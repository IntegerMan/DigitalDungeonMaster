namespace MattEland.DigitalDungeonMaster.Services;

public interface IRecordStorageService
{
    Task<bool> UserExistsAsync(string? username);
    Task CreateTableEntryAsync(string users, IDictionary<string, object> values);
    Task<IEnumerable<TOutput>> GetDataAsync<TOutput>(string tableName, Func<IDictionary<string, object?>, TOutput> mapper);
    Task<IEnumerable<TOutput>> GetPartitionedDataAsync<TOutput>(string tableName, string partitionKey, Func<IDictionary<string, object?>, TOutput> mapper);
    Task<TOutput?> FindByKeyAsync<TOutput>(string tableName, string partitionKey, string rowKey, Func<IDictionary<string, object?>, TOutput> mapper);
    Task<UserInfo?> FindUserAsync(string username);
}