using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.GameManagement.Services;

public class RulesetService
{
    private readonly IStorageService _storageService;
    private readonly ILogger<RulesetService> _logger;

    public RulesetService(IStorageService storageService, ILogger<RulesetService> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Ruleset>> LoadRulesetsAsync(string username)
    {
        // TODO: Move this mapping to the service layer
        List<Ruleset> rulesets = (await _storageService.GetDataAsync("rulesets", entity => new Ruleset
            {
                Name = (string)entity["Name"]!,
                Description = (string?)entity["Description"],
                Owner = (string)entity["PartitionKey"]!,
                Key = (string)entity["RowKey"]!
            }))
            .Where(r => r.Owner == username || r.Owner == "shared")
            .OrderBy(r => r.Name)
            .ToList();
        
        _logger.LogDebug("Loaded {Count} rulesets for {Username}", rulesets.Count, username);
        
        return rulesets;
    }
}