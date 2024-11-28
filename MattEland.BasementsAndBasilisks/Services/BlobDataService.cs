using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;

namespace MattEland.BasementsAndBasilisks.Services;

public class BlobDataService
{
    private readonly TableServiceClient _tableClient;
    private readonly string _adventuresTableName;


    public BlobDataService(BasiliskConfig config)
    {
        _adventuresTableName = config.AdventuresTableName;
        _tableClient = new TableServiceClient(config.AzureStorageTableConnectionString);
    }

    public async Task<IEnumerable<AdventureInfo>> LoadAdventuresAsync(string username)
    {
        TableClient tableClient = _tableClient.GetTableClient("adventures");
        List<TableEntity> results = await tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{username}'").ToListAsync();

        // List row keys
        return results.Select(r => new AdventureInfo
            {
                RowKey = r.RowKey,
                Name = r.GetString("Name"),
                Description = r.GetString("Description"),
                Container = r.GetString("Container"),
                Ruleset = r.GetString("Ruleset"),
                GameWorld = r.GetString("GameWorld")
            });
    }
}