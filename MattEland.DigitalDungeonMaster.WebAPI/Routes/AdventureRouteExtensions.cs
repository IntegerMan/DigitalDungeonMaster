using System.Security.Claims;
using MattEland.DigitalDungeonMaster.GameManagement.Services;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class AdventureRouteExtensions
{
    public static WebApplication AddAdventureManagement(this WebApplication app)
    {
        app.MapGet("/adventures", async ([FromServices] AdventuresService service, [FromServices] AppUser user) =>
        {
            // Get the user information from the request
            string username = user.Id ?? throw new InvalidOperationException("User ID not found");
            
            var adventures = await service.LoadAdventuresAsync(username);
            
            return Results.Ok(adventures);
        })
        .WithName("GetAdventures")
        .WithDescription("Get a list of in progress adventures for the current user")
        .WithOpenApi()
        .RequireAuthorization();
        
        return app;
    }
}