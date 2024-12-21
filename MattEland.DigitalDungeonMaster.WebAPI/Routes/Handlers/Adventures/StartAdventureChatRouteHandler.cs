using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.Adventures;

public class StartAdventureChatRouteHandler
{
    private readonly ILogger<StartAdventureChatRouteHandler> _logger;
    private readonly string _username;
    private readonly ChatService _chatService;
    private readonly AdventuresService _adventuresService;

    public StartAdventureChatRouteHandler(AdventuresService adventuresService, 
        ChatService chatService, 
        AppUser user, 
        ILogger<StartAdventureChatRouteHandler> logger)
    {
        _adventuresService = adventuresService;
        _chatService = chatService;
        _username = user.Name;
        _logger = logger;
    }
    public async Task<IResult> StartAdventureChatAsync(string adventureName)
    {
        _logger.LogInformation("Starting chat for adventure {AdventureName} for user {User}", adventureName, _username);
        
        // Validate the request
        if (string.IsNullOrWhiteSpace(adventureName))
        {
            return Results.BadRequest("No adventure name was provided");
        }
                
        // Get the adventure information from the path
        AdventureInfo? adventure = await _adventuresService.GetAdventureAsync(_username, adventureName);
        if (adventure == null)
        {
            _logger.LogWarning("Could not find an adventure named {AdventureName} for user {User}", adventureName, _username);
            return Results.NotFound($"Could not find an adventure named {adventureName} for your user.");
        }
        _logger.LogDebug("Found adventure {AdventureName} for user {User} in status {Status}", adventureName, _username, adventure.Status);

        if (adventure.Status == AdventureStatus.Building)
        {
            NewGameSettingInfo? settings = await _adventuresService.LoadStorySettingsAsync(adventure);
            
            if (settings is not { IsValid: true })
            {
                _logger.LogWarning("The adventure {AdventureName} is still being built and cannot be started.", adventureName);
                return Results.BadRequest("The adventure is still being built. Please finish initializing it.");
            }

            await _adventuresService.StartAdventureAsync(adventure);
        }
        
        // Begin the conversation
        IChatResult result = await _chatService.StartChatAsync(adventure);
        
        _logger.LogInformation("Chat started for adventure {AdventureName} for user {User}: {Message}", adventureName, _username, result.Replies?.FirstOrDefault()?.Message);
        
        return Results.Ok(result);
    }
}