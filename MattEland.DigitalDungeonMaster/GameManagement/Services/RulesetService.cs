using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.GameManagement.Services;

public class RulesetService
{
    private readonly StorageDataService _storageService;
    private readonly ILogger<RulesetService> _logger;

    public RulesetService(StorageDataService storageService, ILogger<RulesetService> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Ruleset>> LoadRulesetsAsync(string username)
    {
        List<Ruleset> rulesets = (await _storageService.ListTableEntriesAsync("rulesets", entity => new Ruleset
            {
                Name = entity.GetString("Name"),
                Description = entity.GetString("Description"),
                Owner = entity.PartitionKey,
                Key = entity.RowKey
            }))
            .Where(r => r.Owner == username || r.Owner == "shared")
            .OrderBy(r => r.Name)
            .ToList();
        
        _logger.LogDebug("Loaded {Count} rulesets for {Username}", rulesets.Count, username);
        
        return rulesets;
    }
}