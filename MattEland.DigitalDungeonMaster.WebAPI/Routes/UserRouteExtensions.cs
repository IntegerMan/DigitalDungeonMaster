using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;
using MattEland.DigitalDungeonMaster.WebAPI.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class UserRouteExtensions
{
    public static void AddLoginAndRegisterEndpoints(this WebApplication app)
    {
        app.MapPost("/login", async ([FromBody] LoginBody login, 
                [FromServices] IUserService userService, 
                [FromServices] TokenService tokenService) =>
            {
                bool result = await userService.LoginAsync(login.Username, login.Password);

                if (result)
                {
                    string jwt = tokenService.GenerateJwtString(login.Username);

                    return Results.Ok(jwt);
                }

                return Results.Unauthorized();
            })
            .WithName("Login")
            .WithDescription("Logs an existing user into the system and returns a JWT. Requires a username and password.")
            .WithOpenApi()
            .AllowAnonymous();

        app.MapPost("/register", async ([FromBody] RegisterBody login,
                [FromServices] IUserService userService,
                [FromServices] IOptionsSnapshot<RegistrationSettings> registrationSettings,
                [FromServices] TokenService tokenService) =>
            {
                try
                {
                    // Prevent registration if it's disabled
                    if (registrationSettings.Value.AllowRegistration == false)
                    {
                        return Results.BadRequest(
                            "Registration is currently disabled. Please contact the administrator for access.");
                    }

                    // Actually register
                    await userService.RegisterAsync(login.Username, login.Password);
                    
                    string jwt = tokenService.GenerateJwtString(login.Username);

                    return Results.Ok(jwt);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            })
            .WithName("Register")
            .WithDescription("Registers a new user with the system and returns a JWT. Requires a username and password and may be disabled during preview.")
            .WithOpenApi()
            .AllowAnonymous();
    }
}