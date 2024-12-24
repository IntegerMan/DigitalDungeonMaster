using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.Adventures;
using Microsoft.AspNetCore.Mvc;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class AdventureRouteExtensions
{
    public static void AddAdventuresRouteHandlers(this IServiceCollection services)
    {
        services.AddScoped<GetAdventuresForUserRouteHandler>();
        services.AddScoped<StartAdventureChatRouteHandler>();
        services.AddScoped<AdventureChatRouteHandler>();
    }
    
    public static void AddAdventureEndpoints(this WebApplication app)
    {
        app.MapGet("/adventures", async ([FromServices] GetAdventuresForUserRouteHandler handler) 
                => await handler.GetAdventuresForUserAsync())
            .WithName("GetAdventures")
            .WithDescription("Get a list of in progress adventures for the current user")
            .WithOpenApi()
            .RequireAuthorization();

        app.MapPost("/adventures/{adventureName}", async (
                [FromRoute] string adventureName,
                [FromServices] StartAdventureChatRouteHandler handler) 
                => await handler.StartAdventureChatAsync(adventureName))
            .WithName("StartAdventure")
            .WithDescription("Begins a new session under the current adventure. This will start a new chat with either a recap or with the new adventure")
            .WithOpenApi()
            .RequireAuthorization();
        
        app.MapPost("/adventures/{adventureName}/{conversationId}", async (
                [FromRoute] string adventureName,
                [FromRoute] Guid conversationId,
                [FromBody] GameChatRequest request,
                [FromServices] AdventureChatRouteHandler handler) 
                => await handler.ContinueChatAsync(adventureName, conversationId, request))
            .WithName("AdventureChat")
            .WithDescription("Continues a conversation with the game master for the current adventure")
            .WithOpenApi()
            .RequireAuthorization();
    }
}