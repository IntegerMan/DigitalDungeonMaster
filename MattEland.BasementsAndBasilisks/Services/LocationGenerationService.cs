using MattEland.BasementsAndBasilisks.Models;
using Microsoft.SemanticKernel.TextGeneration;

namespace MattEland.BasementsAndBasilisks.Services;

public class LocationGenerationService
{
    private readonly ITextGenerationService _textGenerator;

    public LocationGenerationService(ITextGenerationService textGenerator)
    {
        _textGenerator = textGenerator;
    }
    
    public async Task<LocationDetails> GenerateLocationAsync(int posX, int posY)
    {
        string namePrompt = "Generate the name of a random location. This location is in a table top role playing game intended to be provided to a dungeon master in order to generate interesting descriptions for the players.";
        TextContent nameContent = await _textGenerator.GetTextContentAsync(namePrompt);
        
        string descPrompt = $"Generate the description of a random location named '{nameContent.Text ?? "Unknown"}'. This location is in a table top role playing game intended to be provided to a dungeon master. Be specific about what's here, what it looks like, possible dangers, and what the player might choose to do.";
        TextContent descriptionContent = await _textGenerator.GetTextContentAsync(descPrompt);
        
        LocationDetails tile = new()
        {
            X = posX,
            Y = posY,
            Name = nameContent.Text ?? "Unknown Location",  
            Description = descriptionContent.Text ?? "No description is available for this location. Please update it with the UpdateLocationDetails function.",
            GameHistory = "No game history is available for this location. Please update it with the UpdateLocationDetails function.",
            PrivateStorytellerNotes = "No private notes are available for this location. You can add some with the UpdateLocationDetails function."
        };

        return tile;
    }
}