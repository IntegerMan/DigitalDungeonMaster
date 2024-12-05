using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.Models;
using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Location Plugin provides information about parts of the world based on their tile identifier.")]
public class LocationPlugin : BasiliskPlugin
{
    private int _currentTileX = 1;
    private int _currentTileY = 1;

    private readonly Dictionary<string, LocationDetails> _tiles = new()
    {
        { "1,0", new LocationDetails
        {
            X = 1,
            Y = 0,
            Name = "Circle of Stones",
            Description = "A circle of stones with a mysterious aura. The stones are covered in ancient runes.",
            GameHistory = "This is where Norrick used the Lumos Shard to focus on the stones and see the vision of the Library of Infinite Knowledge to the far north in the mountains",
            PrivateStorytellerNotes = "If Norrick attempts to use the Lumos Shard here again, he will not be successful. The stones have been drained of their power.",
        }}
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
    public string UpdateLocationDetails(int x, int y, string name, string description, string gameHistory, string privateStorytellerNotes)
    {
        Context.LogPluginCall($"{x},{y}: {name}");

        // Perform the update
        LocationDetails details = new()
        {
            X = x,
            Y = y,
            Name = name,
            Description = description,
            GameHistory = gameHistory,
            PrivateStorytellerNotes = privateStorytellerNotes
        };
        _tiles[$"{x},{y}"] = details;
        
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(details);
        Context.AddBlock(new DiagnosticBlock
        {
            Header = $"{x}, {y} Updated",
            Metadata = json
        });
        
        return $"{x}, {x} Updated";
    }
    
    [KernelFunction(nameof(GetLocationDetails)), 
     Description("Gets information about the specified tile of the game world at these X and Y coordinates.")]
    public LocationDetails GetLocationDetails(int x, int y)
    {
        Context.LogPluginCall($"Tile {x}, {y}");
        
        return GetOrGenerateLocationDetails(x, y);
    }

    private LocationDetails GetOrGenerateLocationDetails(int x, int y)
    {
        if (_tiles.ContainsKey($"{x},{y}"))
        {
            return _tiles[$"{x},{y}"];
        }

        LocationDetails tile = new LocationDetails()
        {
            X = x,
            Y = y,
            Name = "Unknown",
            Description = "This location is unknown. Please update it with the UpdateLocationDetails function.",
            GameHistory = "No game history is available for this location. Please update it with the UpdateLocationDetails function.",
            PrivateStorytellerNotes = "No private notes are available for this location. You can add some with the UpdateLocationDetails function."
        };
        
        _tiles[$"{x},{y}"] = tile;
        
        return tile;
    }
}

