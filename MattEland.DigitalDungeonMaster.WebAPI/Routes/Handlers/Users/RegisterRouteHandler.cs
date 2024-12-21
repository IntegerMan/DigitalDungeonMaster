using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;
using MattEland.DigitalDungeonMaster.WebAPI.Settings;
using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.Users;

public class RegisterRouteHandler
{
    private readonly ILogger<RegisterRouteHandler> _logger;
    private readonly IUserService _userService;
    private readonly IOptionsSnapshot<RegistrationSettings> _registrationSettings;
    private readonly TokenService _tokenService;

    public RegisterRouteHandler(
        IUserService userService,
        ILogger<RegisterRouteHandler> logger,
        IOptionsSnapshot<RegistrationSettings> registrationSettings,
        TokenService tokenService)
    {
        _logger = logger;
        _userService = userService;
        _registrationSettings = registrationSettings;
        _tokenService = tokenService;
    }
    public async Task<IResult> HandleAsync(RegisterBody login)
    {
        try
        {
            _logger.LogInformation("Registration attempt for {Username}", login.Username);
            
            // Prevent registration if it's disabled
            if (_registrationSettings.Value.AllowRegistration == false)
            {
                _logger.LogWarning("Registration is disabled at a configuration level.");
                return Results.BadRequest("Registration is currently disabled. Please contact the administrator for access.");
            }

            // Actually register
            await _userService.RegisterAsync(login.Username, login.Password);
            _logger.LogInformation("Registration succeeded for {Username}", login.Username);        
            
            string jwt = _tokenService.GenerateJwtString(login.Username);

            return Results.Ok(jwt);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Registration failed for {Username}: {Message}", login.Username, ex.Message);
            return Results.BadRequest(ex.Message);
        }
    }
}