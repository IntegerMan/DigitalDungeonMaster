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
    
    public async Task CreateAdventureAsync(NewGameSettingInfo setting, string ruleset, string username)
    {
        string key = setting.CampaignName.Replace(" ", ""); // TODO: Check for restricted characters on blob names
                
        AdventureInfo adventure = new()
        {
            Name = setting.CampaignName,
            Ruleset = ruleset,
            Description = setting.GameSettingDescription,
            Owner = username,
            Container = $"{username}_{key}".ToLowerInvariant(),
            RowKey = key,
            Status = AdventureStatus.Building
        };
        
        _logger.LogInformation("Creating adventure {Adventure}", adventure);
        
        // Add an entry to table storage
        // TODO: A reflection-based approach is probably better here
        await _recordStorage.CreateTableEntryAsync("adventures", new Dictionary<string, object?>
        {
            ["PartitionKey"] = adventure.Owner,
            ["RowKey"] = adventure.RowKey,
            ["Name"] = adventure.Name,
            ["Description"] = adventure.Description,
            ["Container"] = adventure.Container,
            ["Ruleset"] = adventure.Ruleset,
            ["Status"] = adventure.Status.ToString()
        });
        
        await UploadStorySettingsAsync(setting, adventure.Owner, adventure.RowKey);
    }

    public async Task UploadStorySettingsAsync(NewGameSettingInfo setting, string username, string adventureKey)
    {
        _logger.LogInformation("Uploading story settings for {Username} in {AdventureKey}", username, adventureKey);
        
        // Upload the settings to blob storage
        string json = JsonConvert.SerializeObject(setting, Formatting.Indented);
        
        await _fileStorage.UploadAsync("adventures", $"{username}_{adventureKey}/StorySetting.json", json);
    }

    public async Task<IEnumerable<AdventureInfo>> LoadAdventuresAsync(string username)
    {
        // TODO: Move this mapping to the service layer
        List<AdventureInfo> entries = (await _recordStorage.GetPartitionedDataAsync("adventures",
            username,
            MapTableRowToAdventure)).ToList();
        
        _logger.LogDebug("Loaded {Count} adventures for {Username}", entries.Count, username);
        
        return entries;
    }

    private static AdventureInfo MapTableRowToAdventure(IDictionary<string, object?> entity)
    {
        return new AdventureInfo
        {
            Owner = (string)entity["PartitionKey"]!,
            RowKey = (string)entity["RowKey"]!,
            Name = (string)entity["Name"]!,
            Description = (string?)entity["Description"],
            Container = (string)entity["Container"]!,
            Ruleset = (string)entity["Ruleset"]!,
            Status = Enum.Parse<AdventureStatus>((string)entity["Status"]!)
        };
    }

    public async Task<AdventureInfo?> GetAdventureAsync(string username, string adventureName)
    {
        AdventureInfo? adventure = await _recordStorage.FindByKeyAsync("adventures", username, adventureName, 
            MapTableRowToAdventure);

        return adventure;
    }

    public async Task<NewGameSettingInfo?> LoadStorySettingsAsync(AdventureInfo adventure)
    {
        string settingsPath = $"{adventure.Container}/StorySetting.json";
        string? json = await _fileStorage.LoadTextOrDefaultAsync("adventures", settingsPath);
        
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogWarning("No settings found for adventure {Adventure} at {SettingsPath}", adventure, settingsPath);
            return null;
        }
        
        _logger.LogDebug("Settings found for adventure {Adventure} at {SettingsPath}", adventure, settingsPath);
        return JsonConvert.DeserializeObject<NewGameSettingInfo>(json);
    }

    public async Task StartAdventureAsync(AdventureInfo adventure)
    {
        await _recordStorage.CreateTableEntryAsync("adventures", new Dictionary<string, object?>
        {
            ["PartitionKey"] = adventure.Owner,
            ["RowKey"] = adventure.RowKey,
            ["Name"] = adventure.Name,
            ["Description"] = adventure.Description,
            ["Container"] = adventure.Container,
            ["Ruleset"] = adventure.Ruleset,
            ["Status"] = AdventureStatus.ReadyToLaunch.ToString()
        });
    }
}