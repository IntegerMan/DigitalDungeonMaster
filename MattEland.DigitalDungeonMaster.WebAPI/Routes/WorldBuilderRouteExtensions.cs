using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class WorldBuilderRouteExtensions
{
    public static void AddWorldBuilderEndpoints(this WebApplication app)
    {
        app.MapPost("/adventures", async (
                [FromBody] AdventureInfo adventure,
                [FromServices] ChatService chatService,
                [FromServices] AdventuresService adventuresService, 
                [FromServices] RulesetService rulesetService,
                [FromServices] IRecordStorageService recordStorage,
                [FromServices] ILogger<Program> logger, // TODO: A more specific class would be better, but I can't do it in an extension method
                [FromServices] AppUser user) =>
            {
                // Validate
                if (string.IsNullOrWhiteSpace(adventure.RowKey))
                {
                    return Results.BadRequest("Please specify an adventure name");
                }
                if (adventure.Owner != user.Name)
                {
                    return Results.Forbid();
                }
                
                /*
                AdventureInfo? match = await adventuresService.GetAdventureAsync(user.Name, adventure.RowKey);
                if (match != null)
                {
                    return Results.Conflict($"An adventure named {adventure.RowKey} already exists for your user");
                }
                */
                
                Ruleset? ruleset = await rulesetService.GetRulesetAsync(user.Name, adventure.Ruleset);
                if (ruleset == null)
                {
                    return Results.BadRequest($"Could not find a ruleset named {adventure.Ruleset} for your user");
                }
                
                // Create the game record
                NewGameSettingInfo setting = new()
                {
                    CampaignName = adventure.Name
                };
                await adventuresService.CreateAdventureAsync(setting, ruleset.Key, user.Name);
                
                // Begin the conversation
                ChatResult<NewGameSettingInfo> result = await chatService.StartWorldBuilderChatAsync(adventure, setting);
                return Results.Ok(result);
            })
            .WithName("StartBuildingAdventure")
            .WithDescription("Begins a new WorldBuilder session to scaffold out a new adventure. This takes in core information about the adventure and returns a chat session.")
            .WithOpenApi()
            .RequireAuthorization();        
        
        app.MapPost("/adventures/{adventureName}/builder/{conversationId}", async (
                [FromBody] ChatRequest<NewGameSettingInfo> chatRequest,
                [FromRoute] Guid conversationId,
                [FromRoute] string adventureName,
                [FromServices] ChatService chatService,
                [FromServices] AdventuresService adventuresService, 
                [FromServices] RulesetService rulesetService,
                [FromServices] ILogger<Program> logger, // TODO: A more specific class would be better, but I can't do it in an extension method
                [FromServices] AppUser user) =>
            {
                logger.LogInformation("Continuing building adventure {AdventureName} with conversation {ConversationId}: {Message}", adventureName, conversationId, chatRequest.Message);
                if (chatRequest.Data is null)
                {
                    return Results.BadRequest("No data was provided");
                }
                logger.LogDebug("Request Data: {Data}", JsonConvert.SerializeObject(chatRequest.Data, Formatting.Indented));

                // Validate
                if (string.IsNullOrWhiteSpace(adventureName))
                {
                    return Results.BadRequest("Please specify an adventure name");
                }
                AdventureInfo? adventure = await adventuresService.GetAdventureAsync(user.Name, adventureName);
                if (adventure == null)
                {
                    return Results.Conflict($"You don't belong to an adventure named {adventureName}");
                }
                if (adventure.Owner != user.Name)
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
                ChatResult<NewGameSettingInfo> result = await chatService.ContinueWorldBuilderChatAsync(chatRequest, adventure);
                
                logger.LogInformation("Response: {Response}", result.Replies?.FirstOrDefault()?.Message ?? "No response");
                logger.LogDebug("Response Data: {Data}", JsonConvert.SerializeObject(result.Data, Formatting.Indented));

                return Results.Ok(result);
            })
            .WithName("ContinueBuildingAdventure")
            .WithDescription("Continues a WorldBuilder session detailing an adventure being constructed")
            .WithOpenApi()
            .RequireAuthorization();
    }
}