using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Services;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.Users;

public class LoginRouteHandler
{
    private readonly TokenService _tokenService;
    private readonly ILogger<LoginRouteHandler> _logger;
    private readonly IUserService _userService;

    public LoginRouteHandler(
        IUserService userService,
        TokenService tokenService,
        ILogger<LoginRouteHandler> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
    }
    public async Task<IResult> HandleAsync(LoginBody login)
    {
        _logger.LogInformation("Login attempt for {Username}", login.Username);
        bool result = await _userService.LoginAsync(login.Username, login.Password);

        if (result)
        {
            _logger.LogInformation("Login succeeded for {Username}", login.Username);
            string jwt = _tokenService.GenerateJwtString(login.Username);

            return Results.Ok(jwt);
        }

        _logger.LogWarning("Login failed for {Username}", login.Username);
        return Results.Unauthorized();
    }
}