using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.Services.Azure;

public class AzureTableStorageService : IRecordStorageService
{
    private readonly ILogger<AzureTableStorageService> _logger;
    private readonly TableServiceClient _tableClient;

    public AzureTableStorageService(IOptionsSnapshot<AzureResourceConfig> config, ILogger<AzureTableStorageService> logger)
    {
        _logger = logger;
        _tableClient = new TableServiceClient(config.Value.AzureStorageConnectionString);
    }

    public async Task<IEnumerable<TOutput>> GetPartitionedDataAsync<TOutput>(string tableName, string partitionKey, Func<IDictionary<string, object?>, TOutput> mapper)
    {
        _logger.LogDebug("Listing Table Resources in Partition: {Table}, {PartitionKey}", tableName, partitionKey);

        TableClient tableClient = _tableClient.GetTableClient(tableName);
        List<TableEntity> results = await tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{partitionKey}'").ToListAsync();
        
        return MapResults(mapper, results);
    }

    public async Task<TOutput?> FindByKeyAsync<TOutput>(string tableName, string partitionKey, string rowKey, Func<IDictionary<string, object?>, TOutput> mapper)
    {
        _logger.LogDebug("Find Resource '{rowKey}' in partition '{PartitionKey}' in table '{Table}'", rowKey, partitionKey, tableName);
        
        TableClient tableClient = _tableClient.GetTableClient(tableName);
        NullableResponse<TableEntity>? entity = await tableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey);

        if (!entity.HasValue)
        {
            return default;
        }

        return mapper(entity.Value!);
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

    public async Task<UserInfo?> FindUserAsync(string username)
    {
        _logger.LogDebug("Getting User: {Username}", username);
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
            IsAdmin = result.Value.GetBoolean("IsAdmin") ?? false
        };
    }

    public async Task CreateTableEntryAsync(string tableName, IDictionary<string, object> values)
    {
        _logger.LogInformation("Creating Table Entry: {Table}, {Entity}", tableName, values);
        
        TableClient tableClient = _tableClient.GetTableClient(tableName);

        TableEntity tableEntity = new TableEntity(values);
        await tableClient.AddEntityAsync(tableEntity);
    }
}