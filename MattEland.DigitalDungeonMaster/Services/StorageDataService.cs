using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using MattEland.DigitalDungeonMaster.Blocks;
using MattEland.DigitalDungeonMaster.Models;
using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.Services;

public class StorageDataService
{
    private readonly RequestContextService _context;
    private readonly TableServiceClient _tableClient;
    private readonly BlobServiceClient _blobClient;

    public StorageDataService(IOptionsSnapshot<AzureResourceConfig> config, RequestContextService context)
    {
        _context = context;
        _tableClient = new TableServiceClient(config.Value.AzureStorageConnectionString);
        _blobClient = new BlobServiceClient(config.Value.AzureStorageConnectionString);
    }

    public async Task<IEnumerable<AdventureInfo>> LoadAdventuresAsync(string username) 
        => await ListTableEntriesInPartitionAsync<AdventureInfo>("adventures",
            username, 
            entity => new AdventureInfo
            {
                RowKey = entity.RowKey,
                Name = entity.GetString("Name"),
                Description = entity.GetString("Description"),
                Container = entity.GetString("Container"),
                Ruleset = entity.GetString("Ruleset")
            });

    internal async Task<IEnumerable<TOutput>> ListTableEntriesInPartitionAsync<TOutput>(string tableName, 
        string partitionKey, 
        Func<TableEntity, TOutput> func)
    {
        _context.AddBlock(new DiagnosticBlock
        {
            Header = "Listing Table Resources in Partition",
            Metadata = $"Table: {tableName}, PartitionKey: {partitionKey}"
        });
        
        TableClient tableClient = _tableClient.GetTableClient(tableName);
        List<TableEntity> results = await tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{partitionKey}'").ToListAsync();
        
        return results.Select(func);
    }
    
    internal async Task<IEnumerable<TOutput>> ListTableEntriesAsync<TOutput>(string tableName,
        Func<TableEntity, TOutput> func)
    {
        _context.AddBlock(new DiagnosticBlock
        {
            Header = "Listing All Table Resources",
            Metadata = $"Table: {tableName}"
        });
        
        TableClient tableClient = _tableClient.GetTableClient(tableName);
        List<TableEntity> results = await tableClient.QueryAsync<TableEntity>().ToListAsync();
        
        return results.Select(func);
    }
    
    internal async Task<bool> UserExistsAsync(string? username)
    {
        _context.AddBlock(new DiagnosticBlock
        {
            Header = "Checking User Existence",
            Metadata = $"Username: {username}"
        });
        
        TableClient tableClient = _tableClient.GetTableClient("users");
        NullableResponse<TableEntity> result = await tableClient.GetEntityIfExistsAsync<TableEntity>(username, username);
        
        return result.HasValue;
    }

    public async Task<string> LoadTextAsync(string container, string path)
    {
        _context.AddBlock(new DiagnosticBlock
        {
            Header = "Loading Text Resource",
            Metadata = $"Container: {container}, Path: {path}"
        });

        BlobContainerClient containerClient = _blobClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);
        
        return await ReadBlobDataAsync(blobClient);
    }
    
    public async Task<string?> LoadTextOrDefaultAsync(string container, string path)
    {
        _context.AddBlock(new DiagnosticBlock
        {
            Header = "Loading Text Resource",
            Metadata = $"Container: {container}, Path: {path}"
        });

        BlobContainerClient containerClient = _blobClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);

        bool exists = await blobClient.ExistsAsync();
        if (!exists)
        {
            return null;
        }
        
        return await ReadBlobDataAsync(blobClient);
    }

    private static async Task<string> ReadBlobDataAsync(BlobClient blobClient)
    {
        // Read all the text of the blob to a string
        await using var stream = await blobClient.OpenReadAsync();
        using var reader = new StreamReader(stream);
        string data = await reader.ReadToEndAsync();
        
        // This could be handy in the future for additional diagnostics: _context.AddBlock(new TextResourceBlock(path, data));
        
        return data;
    }

    public async Task<(byte[]?, byte[]?)> GetUserSaltAndHash(string username)
    {
        TableClient tableClient = _tableClient.GetTableClient("users");
        NullableResponse<TableEntity> result = await tableClient.GetEntityIfExistsAsync<TableEntity>(username, username);
        
        if (!result.HasValue)
        {
            return (null, null);
        }
        
        return (result.Value!.GetBinary("Salt"), result.Value.GetBinary("Hash"));
    }

    public async Task CreateTableEntryAsync(string tableName, TableEntity tableEntity)
    {
        TableClient tableClient = _tableClient.GetTableClient(tableName);
        
        await tableClient.AddEntityAsync(tableEntity);
    }
}