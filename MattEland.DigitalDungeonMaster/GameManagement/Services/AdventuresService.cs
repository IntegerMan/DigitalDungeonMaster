using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.GameManagement.Services;

public class AdventuresService
{
    private readonly IStorageService _storageService;
    private readonly RequestContextService _context;
    private readonly ILogger<AdventuresService> _logger;

    public AdventuresService(IStorageService storageService, RequestContextService context, ILogger<AdventuresService> logger)
    {
        _storageService = storageService;
        _context = context;
        _logger = logger;
    }
    
    public async Task CreateAdventureAsync(NewGameSettingInfo setting, string ruleset)
    {
        string key = setting.CampaignName.Replace(" ", ""); // TODO: Check for restricted characters on blob names
                
        AdventureInfo adventure = new()
        {
            Name = setting.CampaignName,
            Ruleset = ruleset,
            Description = setting.GameSettingDescription,
            Owner = _context.CurrentUser!,
            Container = $"{_context.CurrentUser!}_{key}",
            RowKey = key
        };
        
        _logger.LogInformation("Creating adventure {Adventure}", adventure);
        
        // Add an entry to table storage
        // TODO: A reflection-based approach is probably better here
        await _storageService.CreateTableEntryAsync("adventures", new Dictionary<string, object>
        {
            ["PartitionKey"] = adventure.Owner,
            ["RowKey"] = adventure.RowKey,
            ["Name"] = adventure.Name,
            ["Description"] = adventure.Description,
            ["Container"] = adventure.Container,
            ["Ruleset"] = adventure.Ruleset
        });
        
        // Upload the settings to blob storage
        string json = JsonConvert.SerializeObject(setting, Formatting.Indented);
        await _storageService.UploadAsync(adventure.Container, $"{adventure.Container}/StorySetting.json", json);
        
        // Set our current adventure to this adventure
        _context.CurrentAdventure = adventure;
    }

    public async Task<IEnumerable<AdventureInfo>> LoadAdventuresAsync(string username)
    {
        // TODO: Move this mapping to the service layer
        List<AdventureInfo> entries = (await _storageService.GetPartitionedDataAsync<AdventureInfo>("adventures",
            username,
            entity => new AdventureInfo
            {
                RowKey = (string)entity["RowKey"]!,
                Name = (string)entity["Name"]!,
                Description = (string?)entity["Description"],
                Container = (string)entity["Container"]!,
                Ruleset = (string)entity["Ruleset"]!
            })).ToList();
        
        _logger.LogDebug("Loaded {Count} adventures for {Username}", entries.Count, username);
        
        return entries;
    }
}