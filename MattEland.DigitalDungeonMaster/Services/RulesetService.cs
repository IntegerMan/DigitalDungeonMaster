using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.Services;

public class RulesetService
{
    private readonly IRecordStorageService _storageService;
    private readonly ILogger<RulesetService> _logger;

    public RulesetService(IRecordStorageService storageService, ILogger<RulesetService> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Ruleset>> LoadRulesetsAsync(string username)
    {
        // TODO: Move this mapping to the service layer
        List<Ruleset> rulesets = (await _storageService.GetDataAsync("rulesets", MapTableEntityToRuleset))
            .Where(r => r.Owner == username || r.Owner == "shared")
            .OrderBy(r => r.Name)
            .ToList();
        
        _logger.LogDebug("Loaded {Count} rulesets for {Username}", rulesets.Count, username);
        
        return rulesets;
    }

    private static Ruleset MapTableEntityToRuleset(IDictionary<string, object?> entity)
    {
        return new Ruleset
        {
            Name = (string)entity["Name"]!,
            Owner = (string)entity["PartitionKey"]!,
            Key = (string)entity["RowKey"]!
        };
    }

    public async Task<Ruleset?> GetRulesetAsync(string username, string rulesetName)
    {
        while (true)
        {
            _logger.LogDebug("Attempting to load ruleset {Ruleset} for {Username}", rulesetName, username);
            
            Ruleset? userRuleset = await _storageService.FindByKeyAsync("rulesets", username, rulesetName, MapTableEntityToRuleset);

            if (userRuleset != null)
            {
                _logger.LogDebug("Loaded ruleset {Ruleset} for {Username}", rulesetName, username);
                return userRuleset;
            }
            
            // If we failed to get a specific variant for this user, try shared rulesets
            if (username != "shared")
            {
                username = "shared";
                continue;
            }

            _logger.LogWarning("Could not find ruleset {Ruleset} for {Username}", rulesetName, username);
            
            return null;
        }
    }
}