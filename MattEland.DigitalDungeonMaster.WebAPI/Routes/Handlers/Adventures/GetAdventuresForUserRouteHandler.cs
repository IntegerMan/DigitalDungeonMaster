using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.Adventures;

public class GetAdventuresForUserRouteHandler
{
    private readonly ILogger<GetAdventuresForUserRouteHandler> _logger;
    private readonly AdventuresService _adventuresService;
    private readonly string _username;

    public GetAdventuresForUserRouteHandler(AdventuresService adventuresService, AppUser user, ILogger<GetAdventuresForUserRouteHandler> logger)
    {
        _logger = logger;
        _adventuresService = adventuresService;
        _username = user.Name;
    }
    
    public async Task<IResult> GetAdventuresForUserAsync()
    {
        _logger.LogInformation("Loading adventures for {Username}", _username);
        
        IEnumerable<AdventureInfo> adventures = await _adventuresService.LoadAdventuresAsync(_username);

        return Results.Ok(adventures);
    }
}