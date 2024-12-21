using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.WorldBuilder;
using Microsoft.AspNetCore.Mvc;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class WorldBuilderRouteExtensions
{
    public static void AddWorldBuilderRouteHandlers(this IServiceCollection services)
    {
        services.AddScoped<WorldBuilderChatRouteHandler>();
        services.AddScoped<WorldBuilderStartBuildingRouteHandler>();
    }
    
    public static void AddWorldBuilderEndpoints(this WebApplication app)
    {
        app.MapPost("/adventures", async (
                [FromBody] AdventureInfo adventure,
                [FromServices] WorldBuilderStartBuildingRouteHandler handler) 
                => await handler.StartWorldBuilderAsync(adventure))
            .WithName("StartBuildingAdventure")
            .WithDescription("Begins a new WorldBuilder session to scaffold out a new adventure. This takes in core information about the adventure and returns a chat session.")
            .WithOpenApi()
            .RequireAuthorization();
        
        app.MapPost("/adventures/{adventureName}/builder/{conversationId}", async (
                [FromBody] ChatRequest<NewGameSettingInfo> chatRequest,
                [FromRoute] Guid conversationId,
                [FromRoute] string adventureName,
                [FromServices] WorldBuilderChatRouteHandler handler) 
                => await handler.HandleAsync(chatRequest, conversationId, adventureName))
            .WithName("ContinueBuildingAdventure")
            .WithDescription("Continues a WorldBuilder session detailing an adventure being constructed")
            .WithOpenApi()
            .RequireAuthorization();
    }
}