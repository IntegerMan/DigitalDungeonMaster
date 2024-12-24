using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.Adventures;

public class AdventureChatRouteHandler
{
    private readonly ILogger<AdventureChatRouteHandler> _logger;
    private readonly ChatService _chatService;
    private readonly AdventuresService _adventuresService;
    private readonly string _username;

    public AdventureChatRouteHandler(ILogger<AdventureChatRouteHandler> logger, 
        ChatService chatService, 
        AdventuresService adventuresService,
        AppUser user)
    {
        _logger = logger;
        _chatService = chatService;
        _adventuresService = adventuresService;
        _username = user.Name;
    }
    
    public async Task<IResult> ContinueChatAsync(string adventureName, Guid chatId, GameChatRequest request)
    {
        _logger.LogInformation("Continuing chat for adventure {AdventureName} for user {User}: {Message}", adventureName, _username, request.Message);
        
        // Validate the request
        if (string.IsNullOrWhiteSpace(adventureName))
        {
            return Results.BadRequest("No adventure name was provided");
        }
        if (chatId == Guid.Empty)
        {
            return Results.BadRequest("No conversation ID was provided");
        }
        if (string.IsNullOrWhiteSpace(request.Message.Message))
        {
            return Results.BadRequest("No message was provided");
        }
                
        // Get the adventure information from the path
        AdventureInfo? adventure = await _adventuresService.GetAdventureAsync(_username, adventureName);
        if (adventure == null)
        {
            _logger.LogWarning("Could not find an adventure named {AdventureName} for user {User}", adventureName, _username);
            return Results.NotFound($"Could not find an adventure named {adventureName} for your user.");
        }
        _logger.LogDebug("Found adventure {AdventureName} for user {User} in status {Status}", adventureName, _username, adventure.Status);

        // Begin the conversation
        IChatResult result = await _chatService.ChatAsync(adventure, request);
        
        _logger.LogInformation("Chat response for adventure {AdventureName} for user {User}: {Message}", adventureName, _username, result.Replies.FirstOrDefault()?.Message);
        
        return Results.Ok(result);
    }
}