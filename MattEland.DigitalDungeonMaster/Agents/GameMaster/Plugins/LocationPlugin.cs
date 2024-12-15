using MattEland.DigitalDungeonMaster.Agents.GameMaster.Models;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Location Plugin provides information about parts of the world based on their tile identifier.")]
public class LocationPlugin
{
    private readonly LocationGenerationService _locationGenerator;
    private readonly ILogger<LocationPlugin> _logger;
    private int _currentTileX = 1;
    private int _currentTileY = 0;

    private readonly Dictionary<string, LocationDetails> _tiles = new()
    {
    };
    
    public LocationPlugin(LocationGenerationService locationGenerator, ILogger<LocationPlugin> logger) 
    {
        _locationGenerator = locationGenerator;
        _logger = logger;
    }
    
    [KernelFunction(nameof(GetCurrentLocation)), 
     Description("Gets the current x and y coordinates of the player, in addition to the location's description.")]
    public async Task<string> GetCurrentLocation()
    {
        _logger.LogDebug("{Plugin}-{Method} called at {X},{Y}", nameof(LocationPlugin), nameof(GetCurrentLocation), _currentTileX, _currentTileY);

        if (!_tiles.ContainsKey($"{_currentTileX},{_currentTileY}"))
        {
            return $"Current Location: {_currentTileX}, {_currentTileY}: No description available. Please describe this location and call UpdateLocationDetails.";
        }
        
        return $"Current Location: {_currentTileX}, {_currentTileY}: {await GetOrGenerateLocationDetailsAsync(_currentTileX, _currentTileY)}";
    }

    [KernelFunction(nameof(SetCurrentLocation)), 
     Description("Sets the current location of the player to the specified tile. Only call this if the player wants to change locations, not if you're checking a location's details.")]
    public async Task<string> SetCurrentLocation(int x, int y)
    {
        _logger.LogDebug("{Plugin}-{Method} called with {X},{Y}", nameof(LocationPlugin), nameof(SetCurrentLocation),x, y);

        _currentTileX = x;
        _currentTileY = y;
        
        return $"Current Location set to {_currentTileX}, {_currentTileY}: {await GetOrGenerateLocationDetailsAsync(_currentTileX, _currentTileY)}";
    }
    
    [KernelFunction(nameof(UpdateLocationDetails)), 
     Description("Stores a new description for the specified tile. The description is for the DM to understand the full location")]
    public string UpdateLocationDetails(int x, int y, string locationName, string locationDetails, string gameHistory, string privateNotes)
    {
        _logger.LogDebug("{Plugin}-{Method} called for {X},{Y}: {Name}, {Details}, {History}, {Notes}", 
            nameof(LocationPlugin), 
            nameof(UpdateLocationDetails), 
            x, y,
            locationName,
            locationDetails,
            gameHistory,
            privateNotes);
        
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

        return "Location details stored for future reference.";
    }

    private void StoreLocationDetails(int x, int y, LocationDetails details)
    {
        _logger.LogDebug("Location Details for {X}.{Y} updated to: {Details}", x, y, details);
        
        // TODO: This should be tracked in a database or file
        _tiles[$"{x},{y}"] = details;
    }

    [KernelFunction(nameof(GetLocationDetailsAsync)), 
     Description("Gets information about the specified tile of the game world at these X and Y coordinates. A null result means the location needs to be described and set into UpdateLocationDetails.")]
    public async Task<LocationDetails?> GetLocationDetailsAsync(int x, int y)
    {
        _logger.LogDebug("{Plugin}-{Method} called for {X},{Y}", nameof(LocationPlugin), nameof(GetLocationDetailsAsync), x, y);
        
        return await GetOrGenerateLocationDetailsAsync(x, y);
    }

    private async Task<LocationDetails> GetOrGenerateLocationDetailsAsync(int x, int y)
    {
        LocationDetails details;
        
        // See if we've generated it before
        if (_tiles.ContainsKey($"{x},{y}"))
        {
            details = _tiles[$"{x},{y}"];
            _logger.LogTrace("Location Details lookup succeeded for {X}.{Y}: {Details}", x, y, details);

            return details;
        }

        _logger.LogDebug("No data for {X}, {Y}. Generating...", x, y);
        
        details = await _locationGenerator.GenerateLocationAsync(x, y);
        StoreLocationDetails(x, y, details);
        
        return details;
    }
}

