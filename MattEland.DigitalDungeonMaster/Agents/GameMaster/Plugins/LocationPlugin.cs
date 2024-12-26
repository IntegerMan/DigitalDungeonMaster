using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Models;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Location Plugin provides information about parts of the world based on their tile identifier.")]
public class LocationPlugin : PluginBase
{
    private readonly LocationGenerationService _locationGenerator;
    private readonly LocationService _locationService;
    private readonly RequestContextService _context;

    public LocationPlugin(LocationGenerationService locationGenerator, 
        LocationService locationService,
        RequestContextService context,
        ILogger<LocationPlugin> logger)
        : base(logger)
    {
        _locationGenerator = locationGenerator;
        _locationService = locationService;
        _context = context;
    }

    [KernelFunction(nameof(GetCurrentLocation)),
     Description("Gets the current location description, history, and storyteller notes for the player's current location.")]
    public async Task<LocationDetails> GetCurrentLocation()
    {
        string username = _context.CurrentUser!;
        string adventure = _context.CurrentAdventure!.RowKey;
        
        using Activity? activity = LogActivity($"User {username}, Adventure {adventure}");

        LocationInfo location = await _locationService.GetCurrentLocationAsync(username, adventure);
        LocationDetails? locationDetails = await _locationService.GetDetailsOrDefaultAsync(username, adventure, location);
        
        if (locationDetails is null)
        {
            Logger.LogWarning("No location details found for {Username} in {AdventureName}. Generating...", username, adventure);
            
            activity?.AddEvent(new ActivityEvent("Failed current location lookup", 
                tags: new ActivityTagsCollection(new List<KeyValuePair<string, object?>>
                {
                    new("Region", location.Region),
                    new("X", location.X),
                    new("Y", location.Y),
                })
            ));

            locationDetails = await _locationGenerator.GenerateLocationAsync(username, adventure, location);
            
            await _locationService.UpdateLocationDetailsAsync(username, adventure, locationDetails);
        }
        
        AddLocationDetailsTraceInfo(activity, locationDetails);

        return locationDetails;
    }

    private static void AddLocationDetailsTraceInfo(Activity? activity, LocationDetails locationDetails)
    {
        AddLocationInfoTraceInfo(activity, locationDetails.Location);
        
        activity?.AddTag("Name", locationDetails.Name);
        activity?.AddTag("Description", locationDetails.Description);
        activity?.AddTag("History", locationDetails.GameHistory);
        activity?.AddTag("Notes", locationDetails.StorytellerNotes);
    }

    private static void AddLocationInfoTraceInfo(Activity? activity, LocationInfo location)
    {
        activity?.AddTag("Region", location.Region);
        activity?.AddTag("X", location.X);
        activity?.AddTag("Y", location.Y);
    }

    [KernelFunction(nameof(SetCurrentLocation)),
     Description("Sets the current location of the player to the specified tile. Only call this if the player wants to change locations, not if you're checking a location's details.")]
    public async Task<string> SetCurrentLocation(int x, int y, string? region = null)
    {
        string username = _context.CurrentUser!;
        string adventure = _context.CurrentAdventure!.RowKey;

        using Activity? activity = LogActivity($"New Location: {x}, {y} ({region})");;
        
        LocationInfo location = await ClarifyLocationRegionAsync(x, y, region, username, adventure);

        AddLocationInfoTraceInfo(activity, location);
        
        return $"Location updated to {location}";
    }

    private async Task<LocationInfo> ClarifyLocationRegionAsync(int x, int y, string? region, string username, string adventure)
    {
        LocationInfo location;
        if (string.IsNullOrWhiteSpace(region))
        {
            Logger.LogDebug("No region specified. Using current region for {Username} in {AdventureName}", username, adventure);
            location = await _locationService.GetCurrentLocationAsync(username, adventure);
            location.X = x;
            location.Y = y;
        } else {
            location = new LocationInfo
            {
                Region = region.ToLowerInvariant(),
                X = x,
                Y = y
            };
        }

        return location;
    }

    [KernelFunction(nameof(UpdateLocationName)),
     Description("Stores a new name for the specified location. Names are short identifiers such as 'Cave Entrance' or 'Tower First Floor'.")]
    public async Task<LocationDetails> UpdateLocationName(int x, int y, string locationName, string? region = null)
    {
        using Activity? activity = LogActivity($"Location: {x}, {y} ({region}: {locationName}");

        string username = _context.CurrentUser!;
        string adventure = _context.CurrentAdventure!.RowKey;
        LocationInfo location = await ClarifyLocationRegionAsync(x, y, region, username, adventure);

        LocationDetails? details = await _locationService.GetDetailsOrDefaultAsync(username, adventure, location);
        if (details is null)
        {
            Logger.LogWarning("No location details found for {Username} in {AdventureName}.", username, adventure);
            details = new LocationDetails
            {
                Location = location,
                Name = locationName
            };
        }
        else
        {
            details.Name = locationName;
        }
        
        AddLocationDetailsTraceInfo(activity, details);
        
        await _locationService.UpdateLocationDetailsAsync(username, adventure, details);

        return details;
    }
    
    [KernelFunction(nameof(UpdateLocationDescription)),
     Description("Stores a new detailed description of a location. Detailed descriptions help you keep track of location details over the course of a story.")]
    public async Task<LocationDetails> UpdateLocationDescription(int x, int y, string description, string? region = null)
    {
        using Activity? activity = LogActivity($"Location: {x}, {y} ({region}: {description}");

        string username = _context.CurrentUser!;
        string adventure = _context.CurrentAdventure!.RowKey;
        LocationInfo location = await ClarifyLocationRegionAsync(x, y, region, username, adventure);

        LocationDetails? details = await _locationService.GetDetailsOrDefaultAsync(username, adventure, location);
        if (details is null)
        {
            Logger.LogWarning("No location details found for {Username} in {AdventureName}.", username, adventure);
            details = new LocationDetails
            {
                Location = location,
                Name = "No Description Provided",
                Description = description
            };
        }
        else
        {
            details.Description = description;
        }
        
        AddLocationDetailsTraceInfo(activity, details);
        
        await _locationService.UpdateLocationDetailsAsync(username, adventure, details);

        return details;
    }    
    
    [KernelFunction(nameof(AddLocationHistoryNote)),
     Description("Adds a new historical note for a location. This helps keep track of significant events that have occurred in places over the story.")]
    public async Task<LocationDetails> AddLocationHistoryNote(int x, int y, string historyNote, string? region = null)
    {
        using Activity? activity = LogActivity($"Location: {x}, {y} ({region}: {historyNote}");

        string username = _context.CurrentUser!;
        string adventure = _context.CurrentAdventure!.RowKey;
        LocationInfo location = await ClarifyLocationRegionAsync(x, y, region, username, adventure);

        LocationDetails? details = await _locationService.GetDetailsOrDefaultAsync(username, adventure, location);
        if (details is null)
        {
            Logger.LogWarning("No location details found for {Username} in {AdventureName}.", username, adventure);
            details = new LocationDetails
            {
                Location = location,
                Name = "No Description Provided",
                GameHistory = historyNote
            };
        }
        else
        {
            details.GameHistory = $"{details.GameHistory ?? ""}{Environment.NewLine}{historyNote}".Trim();
        }
        
        AddLocationDetailsTraceInfo(activity, details);
        
        await _locationService.UpdateLocationDetailsAsync(username, adventure, details);

        return details;
    }    
    
    [KernelFunction(nameof(AddLocationStoryNote)),
     Description("Adds a new storyteller note for a location. This helps keep track of secret and planned events that the player shouldn't know about yet.")]
    public async Task<LocationDetails> AddLocationStoryNote(int x, int y, string storyNote, string? region = null)
    {
        using Activity? activity = LogActivity($"Location: {x}, {y} ({region}: {storyNote}");

        string username = _context.CurrentUser!;
        string adventure = _context.CurrentAdventure!.RowKey;
        LocationInfo location = await ClarifyLocationRegionAsync(x, y, region, username, adventure);

        LocationDetails? details = await _locationService.GetDetailsOrDefaultAsync(username, adventure, location);
        if (details is null)
        {
            Logger.LogWarning("No location details found for {Username} in {AdventureName}.", username, adventure);
            details = new LocationDetails
            {
                Location = location,
                Name = "No Description Provided",
                StorytellerNotes = storyNote
            };
        }
        else
        {
            details.StorytellerNotes = $"{details.StorytellerNotes ?? ""}{Environment.NewLine}{storyNote}".Trim();
        }
        
        AddLocationDetailsTraceInfo(activity, details);
        
        await _locationService.UpdateLocationDetailsAsync(username, adventure, details);

        return details;
    }

    [KernelFunction(nameof(GetLocationDetailsAsync)),
     Description("Gets information about the specified tile of the game world at these X and Y coordinates.")]
    public async Task<LocationDetails?> GetLocationDetailsAsync(int x, int y, string? region = null)
    {
        string username = _context.CurrentUser!;
        string adventure = _context.CurrentAdventure!.RowKey;
        
        using Activity? activity = LogActivity($"Location: {x}, {y} ({region})");

        LocationInfo location = await ClarifyLocationRegionAsync(x, y, region, username, adventure);

        LocationDetails? details = await _locationService.GetDetailsOrDefaultAsync(username, adventure, location);
        
        if (details is null)
        {
            Logger.LogWarning("No location details found for {Username} in {AdventureName}.", username, adventure);
            details = new LocationDetails
            {
                Location = location,
                Name = "No Description Provided"
            };
        }
        
        AddLocationDetailsTraceInfo(activity, details);

        return details;
    }
}