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
        List<Ruleset> rulesets = (await _storageService.GetDataAsync("rulesets", entity => new Ruleset
            {
                Name = (string)entity["Name"]!,
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