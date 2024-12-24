using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.Adventures;

public class StartAdventureChatRouteHandler(
    AdventuresService adventuresService,
    ChatService chatService,
    AppUser user,
    ILogger<StartAdventureChatRouteHandler> logger)
{
    private readonly string _username = user.Name;

    public async Task<IResult> StartAdventureChatAsync(string adventureName)
    {
        logger.LogInformation("Starting chat for adventure {AdventureName} for user {User}", adventureName, _username);
        
        // Validate the request
        if (string.IsNullOrWhiteSpace(adventureName))
        {
            return Results.BadRequest("No adventure name was provided");
        }
                
        // Get the adventure information from the path
        AdventureInfo? adventure = await adventuresService.GetAdventureAsync(_username, adventureName);
        if (adventure == null)
        {
            logger.LogWarning("Could not find an adventure named {AdventureName} for user {User}", adventureName, _username);
            return Results.NotFound($"Could not find an adventure named {adventureName} for your user.");
        }
        logger.LogDebug("Found adventure {AdventureName} for user {User} in status {Status}", adventureName, _username, adventure.Status);

        if (adventure.Status == AdventureStatus.Building)
        {
            NewGameSettingInfo? settings = await adventuresService.LoadStorySettingsAsync(adventure);
            
            if (settings is not { IsValid: true })
            {
                logger.LogWarning("The adventure {AdventureName} is still being built and cannot be started.", adventureName);
                return Results.BadRequest("The adventure is still being built. Please finish initializing it.");
            }

            await adventuresService.StartAdventureAsync(adventure);
        }
        
        // Begin the conversation
        GameChatResult result = await chatService.StartChatAsync(adventure);
        
        logger.LogInformation("Chat started for adventure {AdventureName} for user {User}: {Message}", adventureName, _username, result.Replies?.FirstOrDefault()?.Message);
        
        return Results.Ok(result);
    }
}