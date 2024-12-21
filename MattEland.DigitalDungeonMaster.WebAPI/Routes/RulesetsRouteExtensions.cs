using MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.Rulesets;
using Microsoft.AspNetCore.Mvc;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class RulesetsRouteExtensions
{
    public static void AddRulesetRouteHandlers(this IServiceCollection services)
    {
        services.AddScoped<GetRulesetsRouteHandler>();
    }
    
    public static void AddRulesetsEndpoints(this WebApplication app)
    {
        app.MapGet("/rulesets", async ([FromServices] GetRulesetsRouteHandler handler) 
                => await handler.GetRulesetsAsync())
            .WithName("GetRulesets")
            .WithDescription("Gets a list of rulesets available to the current user.")
            .WithOpenApi()
            .RequireAuthorization();
    }
}