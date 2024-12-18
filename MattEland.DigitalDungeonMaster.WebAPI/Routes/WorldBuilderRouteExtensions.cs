using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

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
                
                AdventureInfo? match = await adventuresService.GetAdventureAsync(user.Name, adventure.RowKey);
                if (match != null)
                {
                    return Results.Conflict($"An adventure named {adventure.RowKey} already exists for your user");
                }
                
                Ruleset? ruleset = await rulesetService.GetRulesetAsync(user.Name, adventure.Ruleset);
                if (ruleset == null)
                {
                    return Results.BadRequest($"Could not find a ruleset named {adventure.Ruleset} for your user");
                }
                
                // Begin the conversation
                ChatResult result = await chatService.StartWorldBuilderChatAsync(adventure);
                return Results.Ok(result);
            })
            .WithName("StartBuildingAdventure")
            .WithDescription("Begins a new WorldBuilder session to scaffold out a new adventure. This takes in core information about the adventure and returns a chat session.")
            .WithOpenApi()
            .RequireAuthorization();
    }
}