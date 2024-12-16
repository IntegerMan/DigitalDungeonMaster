using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.GameManagement.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class AdventureRouteExtensions
{
    public static void AddAdventureEndpoints(this WebApplication app)
    {
        app.MapGet("/adventures", async (
                [FromServices] AdventuresService adventuresService, 
                [FromServices] AppUser user) =>
            {
                IEnumerable<AdventureInfo> adventures = await adventuresService.LoadAdventuresAsync(user.Name);

                return Results.Ok(adventures);
            })
            .WithName("GetAdventures")
            .WithDescription("Get a list of in progress adventures for the current user")
            .WithOpenApi()
            .RequireAuthorization();

        app.MapPost("/adventures/{adventureName}", async (
                [FromRoute] string adventureName,
                [FromServices] ChatService chatService,
                [FromServices] AdventuresService adventuresService, 
                [FromServices] ILogger<Program> logger, // TODO: A more specific class would be better, but I can't do it in an extension method
                [FromServices] AppUser user) =>
            {
                // Validate the request
                if (string.IsNullOrWhiteSpace(adventureName))
                {
                    return Results.BadRequest("No adventure name was provided");
                }
                
                // Get the adventure information from the path
                AdventureInfo? adventure = await adventuresService.GetAdventureAsync(user.Name, adventureName);
                if (adventure == null)
                {
                    logger.LogWarning("Could not find an adventure named {AdventureName} for user {User}", adventureName, user.Name);
                    return Results.NotFound($"Could not find an adventure named {adventureName} for your user.");
                }
                logger.LogDebug("Found adventure {AdventureName} for user {User} in status {Status}", adventureName, user.Name, adventure.Status);

                // Begin the conversation
                ChatResult result = await chatService.StartChatAsync(adventure);
                return Results.Ok(result);
            })
            .WithName("StartAdventure")
            .WithDescription("Begins a new session under the current adventure. This will start a new chat with either a recap or with the new adventure")
            .WithOpenApi()
            .RequireAuthorization();
        
        app.MapPost("/adventures/{adventureName}/{conversationId}", async (
                [FromRoute] string adventureName,
                [FromRoute] Guid conversationId,
                [FromBody] ChatRequest request,
                [FromServices] ChatService chatService,
                [FromServices] AdventuresService adventuresService, 
                [FromServices] ILogger<Program> logger, // TODO: A more specific class would be better, but I can't do it in an extension method
                [FromServices] AppUser user) =>
            {
                // Validate the request
                if (string.IsNullOrWhiteSpace(adventureName))
                {
                    return Results.BadRequest("No adventure name was provided");
                }
                if (conversationId == Guid.Empty)
                {
                    return Results.BadRequest("No conversation ID was provided");
                }
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return Results.BadRequest("No message was provided");
                }
                
                // Get the adventure information from the path
                AdventureInfo? adventure = await adventuresService.GetAdventureAsync(user.Name, adventureName);
                if (adventure == null)
                {
                    logger.LogWarning("Could not find an adventure named {AdventureName} for user {User}", adventureName, user.Name);
                    return Results.NotFound($"Could not find an adventure named {adventureName} for your user.");
                }
                logger.LogDebug("Found adventure {AdventureName} for user {User} in status {Status}", adventureName, user.Name, adventure.Status);

                // Begin the conversation
                ChatResult result = await chatService.ChatAsync(adventure, request);
                return Results.Ok(result);
            })
            .WithName("AdventureChat")
            .WithDescription("Continues a conversation with the game master for the current adventure")
            .WithOpenApi()
            .RequireAuthorization();
    }
}