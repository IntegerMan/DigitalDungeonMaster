using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public class WorldBuilderChatRouteHandler
{
    private readonly ILogger<WorldBuilderChatRouteHandler> _logger;
    private readonly AdventuresService _adventuresService;
    private readonly ChatService _chatService;
    private readonly string _username;

    public WorldBuilderChatRouteHandler(ILogger<WorldBuilderChatRouteHandler> logger,
        AppUser user,
        AdventuresService adventuresService,
        ChatService chatService)
    {
        _logger = logger;
        _username = user.Name;
        _adventuresService = adventuresService;
        _chatService = chatService;
    }

    public async Task<IResult> HandleAsync(ChatRequest<NewGameSettingInfo> chatRequest, Guid conversationId, string adventureName)
    {
        _logger.LogInformation(
            "Continuing building adventure {AdventureName} with conversation {ConversationId}: {Message}",
            adventureName, conversationId, chatRequest.Message);
        if (chatRequest.Data is null)
        {
            return Results.BadRequest("No data was provided");
        }

        _logger.LogDebug("Request Data: {Data}", JsonConvert.SerializeObject(chatRequest.Data, Formatting.Indented));

        // Validate
        if (string.IsNullOrWhiteSpace(adventureName))
        {
            return Results.BadRequest("Please specify an adventure name");
        }

        AdventureInfo? adventure = await _adventuresService.GetAdventureAsync(_username, adventureName);
        if (adventure == null)
        {
            return Results.Conflict($"You don't belong to an adventure named {adventureName}");
        }

        if (adventure.Owner != _username)
        {
            return Results.Forbid();
        }

        if (adventure.Status != AdventureStatus.Building)
        {
            return Results.BadRequest($"The {adventureName} adventure has already started");
        }

        if (chatRequest.Id != conversationId)
        {
            return Results.BadRequest("The conversation ID does not match the message");
        }

        if (string.IsNullOrWhiteSpace(chatRequest.Message))
        {
            return Results.BadRequest("Please provide a message");
        }

        // Continue the conversation
        ChatResult<NewGameSettingInfo> result = await _chatService.ContinueWorldBuilderChatAsync(chatRequest, adventure);

        _logger.LogInformation("Response: {Response}", result.Replies?.FirstOrDefault()?.Message ?? "No response");
        _logger.LogDebug("Response Data: {Data}", JsonConvert.SerializeObject(result.Data, Formatting.Indented));

        return Results.Ok(result);
    }
}