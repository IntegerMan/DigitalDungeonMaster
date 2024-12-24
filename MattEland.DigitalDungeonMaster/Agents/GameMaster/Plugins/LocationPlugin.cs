using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Models;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Location Plugin provides information about parts of the world based on their tile identifier.")]
public class LocationPlugin : PluginBase
{
    private readonly LocationGenerationService _locationGenerator;
    private int _currentTileX = 1;
    private int _currentTileY = 0;

    private readonly Dictionary<string, LocationDetails> _tiles = new()
    {
    };

    public LocationPlugin(LocationGenerationService locationGenerator, ILogger<LocationPlugin> logger)
        : base(logger)
    {
        _locationGenerator = locationGenerator;
    }

    [KernelFunction(nameof(GetCurrentLocation)),
     Description("Gets the current x and y coordinates of the player, in addition to the location's description.")]
    public async Task<string> GetCurrentLocation()
    {
        using Activity? activity = LogActivity($"Current Location: {_currentTileX}, {_currentTileY}");

        string result;
        if (!_tiles.ContainsKey($"{_currentTileX},{_currentTileY}"))
        {
            activity?.AddEvent(new ActivityEvent("Failed current location lookup", tags: 
                new ActivityTagsCollection(new List<KeyValuePair<string, object?>>
                {
                    new("X", _currentTileX),
                    new("Y", _currentTileY),
                })
            ));
            
            result = $"No description available. Please describe this location and call {nameof(UpdateLocationDetails)}.";
        }
        else
        {
            result = (await GetOrGenerateLocationDetailsAsync(_currentTileX, _currentTileY, activity)).Description;
        }

        activity?.AddTag("Description", result);

        return $"Current Location: {_currentTileX}, {_currentTileY}: {result}";
    }

    [KernelFunction(nameof(SetCurrentLocation)),
     Description("Sets the current location of the player to the specified tile. Only call this if the player wants to change locations, not if you're checking a location's details.")]
    public async Task<string> SetCurrentLocation(int x, int y)
    {
        using Activity? activity = LogActivity($"New Location: {x}, {y}");

        _currentTileX = x;
        _currentTileY = y;

        activity?.AddTag("X", x);
        activity?.AddTag("Y", y);

        string details = (await GetOrGenerateLocationDetailsAsync(_currentTileX, _currentTileY, activity)).Description;
        activity?.AddTag("Description", details);

        return $"Current Location set to {_currentTileX}, {_currentTileY} with description: {details}";
    }

    [KernelFunction(nameof(UpdateLocationDetails)),
     Description(
         "Stores a new description for the specified tile. The description is for the DM to understand the full location")]
    public string UpdateLocationDetails(int x, int y, string locationName, string locationDetails, string gameHistory,
        string privateNotes)
    {
        using Activity? activity = LogActivity($"Location: {x}, {y}: {locationName}");

        LocationDetails details = new()
        {
            X = x,
            Y = y,
            Name = locationName,
            Description = locationDetails,
            GameHistory = gameHistory,
            PrivateStorytellerNotes = privateNotes
        };

        StoreLocationDetails(x, y, details);
        AddLocationDetailsTags(activity, details);

        return "Location details stored for future reference.";
    }

    private void StoreLocationDetails(int x, int y, LocationDetails details)
    {
        Logger.LogDebug("Location Details for {X}.{Y} updated to: {Details}", x, y, details);

        // TODO: This should be tracked in a database or file
        _tiles[$"{x},{y}"] = details;
    }

    [KernelFunction(nameof(GetLocationDetailsAsync)),
     Description(
         "Gets information about the specified tile of the game world at these X and Y coordinates. A null result means the location needs to be described and set into UpdateLocationDetails.")]
    public async Task<LocationDetails?> GetLocationDetailsAsync(int x, int y)
    {
        using Activity? activity = LogActivity($"Location: {x}, {y}");

        LocationDetails details = await GetOrGenerateLocationDetailsAsync(x, y, activity);
        AddLocationDetailsTags(activity, details);

        return details;
    }

    private static void AddLocationDetailsTags(Activity? activity, LocationDetails details)
    {
        activity?.AddTag("X", details.X);
        activity?.AddTag("Y", details.Y);
        activity?.AddTag("Name", details.Name);
        activity?.AddTag("Description", details.Description);
        activity?.AddTag("GameHistory", details.GameHistory);
        activity?.AddTag("Notes", details.PrivateStorytellerNotes);
    }

    private async Task<LocationDetails> GetOrGenerateLocationDetailsAsync(int x, int y, Activity? activity)
    {
        LocationDetails details;

        // See if we've generated it before
        if (_tiles.ContainsKey($"{x},{y}"))
        {
            details = _tiles[$"{x},{y}"];
            activity?.AddEvent(new ActivityEvent("Successful location lookup", tags: 
                new ActivityTagsCollection(new List<KeyValuePair<string, object?>>
                {
                    new("X", x),
                    new("Y", y),
                    new("Name", details.Name),
                    new("Description", details.Description),
                    new("History", details.GameHistory),
                    new("Notes", details.PrivateStorytellerNotes),
                })
            ));
            Logger.LogTrace("Location Details lookup succeeded for {X}.{Y}: {Details}", x, y, details);

            return details;
        }

        Logger.LogDebug("No data for {X}, {Y}. Generating...", x, y);

        details = await _locationGenerator.GenerateLocationAsync(x, y);
        
        activity?.AddEvent(new ActivityEvent("Location generation", tags: 
            new ActivityTagsCollection(new List<KeyValuePair<string, object?>>
            {
                new("X", x),
                new("Y", y),
                new("Name", details.Name),
                new("Description", details.Description),
                new("History", details.GameHistory),
                new("Notes", details.PrivateStorytellerNotes),
            })
        ));
        
        StoreLocationDetails(x, y, details);

        return details;
    }
}