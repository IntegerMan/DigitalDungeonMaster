using MattEland.DigitalDungeonMaster.Agents.GameMaster.Models;
using Microsoft.SemanticKernel.TextGeneration;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;

public class LocationGenerationService
{
    private readonly ITextGenerationService _textGenerator;
    private readonly ILogger<LocationGenerationService> _logger;

    public LocationGenerationService(ITextGenerationService textGenerator, ILogger<LocationGenerationService> logger)
    {
        _textGenerator = textGenerator;
        _logger = logger;
    }
    
    public async Task<LocationDetails> GenerateLocationAsync(string username, string adventure, LocationInfo location)
    {
        string namePrompt = "Generate the name of a random location. This location is in a table top role playing game intended to be provided to a dungeon master in order to generate interesting descriptions for the players.";
        TextContent nameContent = await _textGenerator.GetTextContentAsync(namePrompt);
        
        string descPrompt = $"Generate the description of a random location named '{nameContent.Text ?? "Unknown"}'. This location is in a table top role playing game intended to be provided to a dungeon master. Be specific about what's here, what it looks like, possible dangers, and what the player might choose to do.";
        TextContent descriptionContent = await _textGenerator.GetTextContentAsync(descPrompt);
        
        LocationDetails tile = new()
        {
            Location = location,
            Name = nameContent.Text ?? "Unknown Location",  
            Description = descriptionContent.Text ?? "No description is available for this location. Please update it with the UpdateLocationDetails function.",
            GameHistory = "No game history is available for this location. Please update it with the UpdateLocationDetails function.",
            StorytellerNotes = "No private notes are available for this location. You can add some with the UpdateLocationDetails function."
        };
        
        _logger.LogInformation("Generated location {Location}", tile);

        return tile;
    }
}