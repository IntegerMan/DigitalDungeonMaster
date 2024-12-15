using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.GameManagement.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class AdventureRouteExtensions
{
    public static void AddAdventureManagementEndpoints(this WebApplication app)
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
                [FromServices] AppUser user) =>
            {
                // Get the user information from the request
                AdventureInfo? adventure = await adventuresService.GetAdventureAsync(user.Name, adventureName);

                if (adventure == null)
                {
                    return Results.NotFound($"Could not find an adventure named {adventureName} for your user.");
                }
                
                ChatResult result = await chatService.StartChatAsync(user.Name, adventure);

                return Results.Ok(result);
            })
            .WithName("StartAdventure")
            .WithDescription("Begins a new session under the current adventure. This will start a new chat with either a recap or with the new adventure")
            .WithOpenApi()
            .RequireAuthorization();
    }
}