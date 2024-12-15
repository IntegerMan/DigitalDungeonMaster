using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes;

public static class UserManagementExtensions
{
    public static JwtSettings GetJwtSettings(this IConfiguration config) 
        => config.GetRequiredSection("JwtSettings").Get<JwtSettings>()!;
    public static WebApplication AddLoginAndRegister(this WebApplication app)
    {
        JwtSettings jwtSettings = app.Configuration.GetJwtSettings();
        
        app.MapPost("/login", async ([FromBody] LoginBody login, [FromServices] IUserService userService) =>
            {
                bool result = await userService.LoginAsync(login.Username, login.Password);

                if (result)
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, login.Username),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    JwtSecurityToken token = new JwtSecurityToken(
                        issuer: jwtSettings.Issuer,
                        audience: jwtSettings.Audience,
                        claims: claims,
                        expires: DateTime.Now.AddHours(6),
                        signingCredentials: creds);

                    string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    return Results.Ok(tokenString);
                }

                return Results.Unauthorized();
            })
            .WithName("Login")
            .AllowAnonymous()
            .WithOpenApi();

        app.MapPost("/register", async ([FromBody] RegisterBody login,
                [FromServices] IUserService userService,
                [FromServices] IOptionsSnapshot<RegistrationSettings> registrationSettings) =>
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

                    return Results.CreatedAtRoute("Login");
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            })
            .WithName("Register")
            .AllowAnonymous()
            .WithOpenApi();

        return app;
    }
}