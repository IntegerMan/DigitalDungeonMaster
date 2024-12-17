using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class RulesetsRouteExtensions
{
    public static void AddRulesetsEndpoints(this WebApplication app)
    {
        app.MapGet("/rulesets", async ([FromServices] RulesetService rulesetService, [FromServices] AppUser user) =>
            {
                IEnumerable<Ruleset> rulesets = await rulesetService.LoadRulesetsAsync(user.Name);

                return Results.Ok(rulesets);
            })
            .WithName("GetRulesets")
            .WithDescription("Gets a list of rulesets available to the current user.")
            .WithOpenApi()
            .RequireAuthorization();
    }
}