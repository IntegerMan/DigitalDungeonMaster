using MattEland.DigitalDungeonMaster.Models;

namespace MattEland.DigitalDungeonMaster.Services;

public class RulesetService
{
    private readonly StorageDataService _storageService;

    public RulesetService(StorageDataService storageService)
    {
        _storageService = storageService;
    }
    
    public async Task<IEnumerable<Ruleset>> LoadRulesetsAsync(string username)
    {
        return (await _storageService.ListTableEntriesAsync("rulesets", entity => new Ruleset
        {
            Name = entity.GetString("Name"),
            Description = entity.GetString("Description"),
            Owner = entity.PartitionKey,
            Key = entity.RowKey
        }))
            .Where(r => r.Owner == username || r.Owner == "shared")
            .OrderBy(r => r.Name);
    }
}