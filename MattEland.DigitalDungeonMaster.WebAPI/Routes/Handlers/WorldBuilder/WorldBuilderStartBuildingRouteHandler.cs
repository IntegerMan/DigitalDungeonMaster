using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public class WorldBuilderStartBuildingRouteHandler
{
    private readonly AdventuresService _adventuresService;
    private readonly ILogger<WorldBuilderStartBuildingRouteHandler> _logger;
    private readonly string _username;
    private readonly RulesetService _rulesetService;
    private readonly ChatService _chatService;

    public WorldBuilderStartBuildingRouteHandler(
        AdventuresService adventuresService,
        ILogger<WorldBuilderStartBuildingRouteHandler> logger,
        AppUser user,
        RulesetService rulesetService,
        ChatService chatService)
    {
        _adventuresService = adventuresService;
        _logger = logger;
        _username = user.Name;
        _rulesetService = rulesetService;
        _chatService = chatService;
    }
    public async Task<IResult> StartWorldBuilderAsync(AdventureInfo adventure)
    {
        _logger.LogInformation("Starting a new adventure named {AdventureName} for user {User}", adventure.RowKey, _username);
        
        // Validate
        if (string.IsNullOrWhiteSpace(adventure.RowKey))
        {
            return Results.BadRequest("Please specify an adventure name");
        }
        if (adventure.Owner != _username)
        {
            _logger.LogWarning("User {User} attempted to chat in an adventure they don't own (owner: {Owner})", _username, adventure.Owner);
            return Results.Forbid();
        }
                
        /*
        AdventureInfo? match = await adventuresService.GetAdventureAsync(user.Name, adventure.RowKey);
        if (match != null)
        {
            return Results.Conflict($"An adventure named {adventure.RowKey} already exists for your user");
        }
        */
                
        Ruleset? ruleset = await _rulesetService.GetRulesetAsync(_username, adventure.Ruleset);
        if (ruleset == null)
        {
            return Results.BadRequest($"Could not find a ruleset named {adventure.Ruleset} for your user");
        }
                
        // Create the game record
        NewGameSettingInfo setting = new()
        {
            CampaignName = adventure.Name
        };
        await _adventuresService.CreateAdventureAsync(setting, ruleset.Key, _username);
                
        // Begin the conversation
        ChatResult<NewGameSettingInfo> result = await _chatService.StartWorldBuilderChatAsync(adventure, setting);
        
        if (result.Data is not null) {
            await _adventuresService.UploadStorySettingsAsync(result.Data, _username, adventure.RowKey);
        }
        
        _logger.LogInformation("Started a new conversation for {AdventureName} with Id {ConversationId}: {Message}", adventure.RowKey, result.Id, result.Replies?.FirstOrDefault()?.Message ?? "No message");
        _logger.LogDebug("Response Data: {Data}", JsonConvert.SerializeObject(result.Data, Formatting.Indented));
        
        return Results.Ok(result);
    }
}