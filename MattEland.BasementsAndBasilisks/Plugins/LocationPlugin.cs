using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Location Plugin provides information about parts of the world based on their tile identifier.")]
public class LocationPlugin : BasiliskPlugin
{
    private int _currentTileX = 1;
    private int _currentTileY = 1;

    private readonly Dictionary<string, string> _tiles = new()
    {
        { "1,1", "The circle of stones. Please update this description with a better description for your future reference"}
    };
    
    public LocationPlugin(RequestContextService context) : base(context)
    {
    }
    
    [KernelFunction(nameof(GetCurrentLocation)), 
     Description("Gets the current x and y coordinates of the player, in addition to the location's description.")]
    public string GetCurrentLocation()
    {
        Context.LogPluginCall($"{_currentTileX},{_currentTileY}");
        
        return $"Current Location: {_currentTileX}, {_currentTileY}: {GetOrGenerateLocationDetails(_currentTileX, _currentTileY)}";
    }

    [KernelFunction(nameof(SetCurrentLocation)), 
     Description("Sets the current location of the player to the specified tile. Only call this if the player wants to change locations, not if you're checking a location's details.")]
    public string SetCurrentLocation(int x, int y)
    {
        Context.LogPluginCall($"{x},{y}");

        _currentTileX = x;
        _currentTileY = y;
        
        return $"Current Location set to {_currentTileX}, {_currentTileY}: {GetOrGenerateLocationDetails(_currentTileX, _currentTileY)}";
    }
    
    [KernelFunction(nameof(UpdateLocationDetails)), 
     Description("Stores a new description for the specified tile. The description is for the DM to understand the full location")]
    public string UpdateLocationDetails(int x, int y, string description)
    {
        Context.LogPluginCall($"{x},{y}: {description}");
        
        _tiles[$"{x},{y}"] = description;
        
        return $"Description of {x}, {x}: {GetOrGenerateLocationDetails(x, y)}";
    }
    
    [KernelFunction(nameof(GetLocationDetails)), 
     Description("Gets information about the specified tile of the game world at these X and Y coordinates.")]
    public string GetLocationDetails(int x, int y)
    {
        Context.LogPluginCall($"Tile {x}, {y}");
        
        return GetOrGenerateLocationDetails(x, y);
    }

    private string GetOrGenerateLocationDetails(int x, int y)
    {
        return _tiles.GetValueOrDefault($"{x},{y}", $"No details available for this location. Please generate them and then call {nameof(UpdateLocationDetails)}");
    }
}