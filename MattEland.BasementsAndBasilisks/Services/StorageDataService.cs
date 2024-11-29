using Azure.Data.Tables;
using MattEland.BasementsAndBasilisks.Blocks;

namespace MattEland.BasementsAndBasilisks.Services;

public class StorageDataService
{
    private readonly RequestContextService _context;
    private readonly TableServiceClient _tableClient;
    
    public StorageDataService(BasiliskConfig config, RequestContextService context)
    {
        _context = context;
        _tableClient = new TableServiceClient(config.AzureStorageTableConnectionString);
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
                Ruleset = entity.GetString("Ruleset"),
                GameWorld = entity.GetString("GameWorld")
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
}