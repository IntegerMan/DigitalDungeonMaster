using System.Text;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.Blocks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.Services;

public class StorageDataService
{
    private readonly ILogger<StorageDataService> _logger;
    private readonly TableServiceClient _tableClient;
    private readonly BlobServiceClient _blobClient;

    public StorageDataService(IOptionsSnapshot<AzureResourceConfig> config, ILogger<StorageDataService> logger)
    {
        _logger = logger;
        _tableClient = new TableServiceClient(config.Value.AzureStorageConnectionString);
        _blobClient = new BlobServiceClient(config.Value.AzureStorageConnectionString);
    }

    internal async Task<IEnumerable<TOutput>> ListTableEntriesInPartitionAsync<TOutput>(string tableName, 
        string partitionKey, 
        Func<TableEntity, TOutput> func)
    {
        _logger.LogDebug("Listing Table Resources in Partition: {Table}, {PartitionKey}", tableName, partitionKey);

        TableClient tableClient = _tableClient.GetTableClient(tableName);
        List<TableEntity> results = await tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{partitionKey}'").ToListAsync();
        
        return results.Select(func);
    }
    
    internal async Task<IEnumerable<TOutput>> ListTableEntriesAsync<TOutput>(string tableName,
        Func<TableEntity, TOutput> func)
    {
        _logger.LogDebug("Listing Table Resources: {Table}", tableName);
        
        TableClient tableClient = _tableClient.GetTableClient(tableName);
        List<TableEntity> results = await tableClient.QueryAsync<TableEntity>().ToListAsync();
        
        return results.Select(func);
    }
    
    internal async Task<bool> UserExistsAsync(string? username)
    {
        _logger.LogDebug("Checking User Existence: {Username}", username);
        
        TableClient tableClient = _tableClient.GetTableClient("users");
        NullableResponse<TableEntity> result = await tableClient.GetEntityIfExistsAsync<TableEntity>(username, username);
        
        return result.HasValue;
    }

    public async Task<string> LoadTextAsync(string container, string path)
    {
        _logger.LogDebug("Loading Text Resource: {Container}, {Path}", container, path);

        BlobContainerClient containerClient = _blobClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);
        
        return await ReadBlobDataAsync(blobClient);
    }
    
    public async Task<string?> LoadTextOrDefaultAsync(string container, string path)
    {
        _logger.LogDebug("Loading Optional Text Resource: {Container}, {Path}", container, path);

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

    internal async Task<(byte[]?, byte[]?)> GetUserSaltAndHash(string username)
    {
        // TODO: This could really be more generic and take in a function to map the entity to the output
        
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
        _logger.LogInformation("Creating Table Entry: {Table}, {Entity}", tableName, tableEntity);
        
        TableClient tableClient = _tableClient.GetTableClient(tableName);
        
        await tableClient.AddEntityAsync(tableEntity);
    }

    public async Task UploadBlobAsync(string container, string path, string content)
    {
        _logger.LogInformation("Uploading Blob: {Container}, {Path}", container, path);
        
        BlobClient blobClient = _blobClient.GetBlobContainerClient(container).GetBlobClient(path);
        
        await using MemoryStream stream = new(Encoding.UTF8.GetBytes(content));
        await blobClient.UploadAsync(stream, overwrite: true);
    }
}