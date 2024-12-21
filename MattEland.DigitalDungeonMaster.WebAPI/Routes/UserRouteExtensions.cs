using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.Users;
using Microsoft.AspNetCore.Mvc;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class UserRouteExtensions
{
    public static void AddUserRouteHandlers(this IServiceCollection services)
    {
        services.AddScoped<LoginRouteHandler>();
        services.AddScoped<RegisterRouteHandler>();
    }
    
    public static void AddLoginAndRegisterEndpoints(this WebApplication app)
    {
        app.MapPost("/login", async ([FromBody] LoginBody login, 
                [FromServices] LoginRouteHandler handler) => await handler.HandleAsync(login))
            .WithName("Login")
            .WithDescription("Logs an existing user into the system and returns a JWT. Requires a username and password.")
            .WithOpenApi()
            .AllowAnonymous();

        app.MapPost("/register", async ([FromBody] RegisterBody login,
                [FromServices] RegisterRouteHandler handler) 
                => await handler.HandleAsync(login))
            .WithName("Register")
            .WithDescription("Registers a new user with the system and returns a JWT. Requires a username and password and may be disabled during preview.")
            .WithOpenApi()
            .AllowAnonymous();
    }
}