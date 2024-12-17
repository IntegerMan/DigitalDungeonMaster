using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.Shared;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.Services;

public class AdventuresService
{
    private readonly IRecordStorageService _recordStorage;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<AdventuresService> _logger;

    public AdventuresService(IRecordStorageService recordStorage, IFileStorageService fileStorage, ILogger<AdventuresService> logger)
    {
        _recordStorage = recordStorage;
        _fileStorage = fileStorage;
        _logger = logger;
    }
    
    public async Task<AdventureInfo> CreateAdventureAsync(NewGameSettingInfo setting, string ruleset, string username)
    {
        string key = setting.CampaignName.Replace(" ", ""); // TODO: Check for restricted characters on blob names
                
        AdventureInfo adventure = new()
        {
            Name = setting.CampaignName,
            Ruleset = ruleset,
            Description = setting.GameSettingDescription,
            Owner = username,
            Container = $"{username}_{key}",
            RowKey = key,
            Status = AdventureStatus.New
        };
        
        _logger.LogInformation("Creating adventure {Adventure}", adventure);
        
        // Add an entry to table storage
        // TODO: A reflection-based approach is probably better here
        await _recordStorage.CreateTableEntryAsync("adventures", new Dictionary<string, object>
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
        await _fileStorage.UploadAsync(adventure.Container, $"{adventure.Container}/StorySetting.json", json);
        
        return adventure;
    }

    public async Task<IEnumerable<AdventureInfo>> LoadAdventuresAsync(string username)
    {
        // TODO: Move this mapping to the service layer
        List<AdventureInfo> entries = (await _recordStorage.GetPartitionedDataAsync<AdventureInfo>("adventures",
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

    public async Task<AdventureInfo?> GetAdventureAsync(string username, string adventureName)
    {
        AdventureInfo? adventure = await _recordStorage.FindByKeyAsync("adventures", username, adventureName, 
            d => new AdventureInfo
            {
                Name = (string)d["Name"]!,
                Description = d["Description"] as string,
                Container = (string)d["Container"]!,
                Ruleset = (string)d["Ruleset"]!,
                Owner = (string)d["PartitionKey"]!,
                RowKey = (string)d["RowKey"]!,
                Status = Enum.Parse<AdventureStatus>((string)d["Status"]!)
            });

        return adventure;
    }
}