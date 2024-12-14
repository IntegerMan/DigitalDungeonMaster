using System.Text;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.Services.Azure;

public class AzureStorageService : IStorageService
{
    private readonly ILogger<AzureStorageService> _logger;
    private readonly TableServiceClient _tableClient;
    private readonly BlobServiceClient _blobClient;

    public AzureStorageService(IOptionsSnapshot<AzureResourceConfig> config, ILogger<AzureStorageService> logger)
    {
        _logger = logger;
        _tableClient = new TableServiceClient(config.Value.AzureStorageConnectionString);
        _blobClient = new BlobServiceClient(config.Value.AzureStorageConnectionString);
    }

    public async Task<IEnumerable<TOutput>> GetPartitionedDataAsync<TOutput>(string tableName, string partitionKey, Func<IDictionary<string, object?>, TOutput> mapper)
    {
        _logger.LogDebug("Listing Table Resources in Partition: {Table}, {PartitionKey}", tableName, partitionKey);

        TableClient tableClient = _tableClient.GetTableClient(tableName);
        List<TableEntity> results = await tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{partitionKey}'").ToListAsync();
        
        return MapResults(mapper, results);
    }

    private static IEnumerable<TOutput> MapResults<TOutput>(Func<IDictionary<string, object?>, TOutput> mapper, List<TableEntity> results)
    {
        return results.Select(e =>
        {
            IDictionary<string, object?> values = new Dictionary<string, object?>();
            foreach (var property in e)
            {
                values[property.Key] = property.Value;
            }

            return mapper(values);
        });
    }

    public async Task<IEnumerable<TOutput>> GetDataAsync<TOutput>(string tableName, Func<IDictionary<string, object?>, TOutput> mapper)
    {
        _logger.LogDebug("Listing Table Resources: {Table}", tableName);
        
        TableClient tableClient = _tableClient.GetTableClient(tableName);
        List<TableEntity> results = await tableClient.QueryAsync<TableEntity>().ToListAsync();
        
        return MapResults(mapper, results);
    }

    public async Task<bool> UserExistsAsync(string? username)
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

    public async Task<UserInfo?> GetUserAsync(string username)
    {
        // TODO: This could really be more generic and take in a function to map the entity to the output
        
        TableClient tableClient = _tableClient.GetTableClient("users");
        NullableResponse<TableEntity> result = await tableClient.GetEntityIfExistsAsync<TableEntity>(username, username);
        
        if (!result.HasValue)
        {
            return null;
        }

        return new UserInfo
        {
            Username = username,
            PasswordSalt = result.Value!.GetBinary("Salt"),
            PasswordHash = result.Value.GetBinary("Hash"),
            IsAdmin = result.Value.GetBoolean("IsAdmin") ?? false,
            Token = result.Value.GetString("Token") ?? Guid.Empty.ToString()
        };
    }

    public async Task CreateTableEntryAsync(string tableName, IDictionary<string, object> values)
    {
        _logger.LogInformation("Creating Table Entry: {Table}, {Entity}", tableName, values);
        
        TableClient tableClient = _tableClient.GetTableClient(tableName);

        TableEntity tableEntity = new TableEntity(values);
        await tableClient.AddEntityAsync(tableEntity);
    }

    public async Task UploadAsync(string container, string path, string content)
    {
        _logger.LogInformation("Uploading Blob: {Container}, {Path}", container, path);
        
        BlobClient blobClient = _blobClient.GetBlobContainerClient(container).GetBlobClient(path);
        
        await using MemoryStream stream = new(Encoding.UTF8.GetBytes(content));
        await blobClient.UploadAsync(stream, overwrite: true);
    }
}