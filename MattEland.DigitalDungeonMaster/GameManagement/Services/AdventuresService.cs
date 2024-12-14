using Azure.Data.Tables;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.GameManagement.Services;

public class AdventuresService
{
    private readonly StorageDataService _storageService;
    private readonly RequestContextService _context;
    private readonly ILogger<AdventuresService> _logger;

    public AdventuresService(StorageDataService storageService, RequestContextService context, ILogger<AdventuresService> logger)
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
        await _storageService.CreateTableEntryAsync("adventures", new TableEntity
        {
            PartitionKey = adventure.Owner,
            RowKey = adventure.RowKey,
            ["Name"] = adventure.Name,
            ["Description"] = adventure.Description,
            ["Container"] = adventure.Container,
            ["Ruleset"] = adventure.Ruleset
        });
        
        // Upload the settings to blob storage
        string json = JsonConvert.SerializeObject(setting, Formatting.Indented);
        await _storageService.UploadBlobAsync(adventure.Container, $"{adventure.Container}/StorySetting.json", json);
        
        // Set our current adventure to this adventure
        _context.CurrentAdventure = adventure;
    }

    public async Task<IEnumerable<AdventureInfo>> LoadAdventuresAsync(string username)
    {
        List<AdventureInfo> entries = (await _storageService.ListTableEntriesInPartitionAsync<AdventureInfo>("adventures",
            username,
            entity => new AdventureInfo
            {
                RowKey = entity.RowKey,
                Name = entity.GetString("Name"),
                Description = entity.GetString("Description"),
                Container = entity.GetString("Container"),
                Ruleset = entity.GetString("Ruleset")
            })).ToList();
        
        _logger.LogDebug("Loaded {Count} adventures for {Username}", entries.Count, username);
        
        return entries;
    }
}