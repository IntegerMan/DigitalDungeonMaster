namespace MattEland.DigitalDungeonMaster.Services;

public interface IStorageService
{
    Task<bool> UserExistsAsync(string? username);
    Task CreateTableEntryAsync(string users, IDictionary<string, object> values);

    Task<IEnumerable<TOutput>> GetDataAsync<TOutput>(string tableName, Func<IDictionary<string, object?>, TOutput> mapper);
    Task<IEnumerable<TOutput>> GetPartitionedDataAsync<TOutput>(string tableName, string partitionKey, Func<IDictionary<string, object?>, TOutput> mapper);
    Task<TOutput?> FindByKeyAsync<TOutput>(string tableName, string partitionKey, string rowKey, Func<IDictionary<string, object?>, TOutput> mapper);
    
    Task UploadAsync(string container, string path, string content);
    Task<string?> LoadTextOrDefaultAsync(string container, string path);
    Task<string> LoadTextAsync(string container, string path);
    Task<UserInfo?> FindUserAsync(string username);
}