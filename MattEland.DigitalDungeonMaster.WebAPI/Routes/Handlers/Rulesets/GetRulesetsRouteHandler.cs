using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;

namespace MattEland.DigitalDungeonMaster.WebAPI.Routes.Handlers.Rulesets;

public class GetRulesetsRouteHandler
{
    private readonly RulesetService _rulesetService;
    private readonly ILogger<GetRulesetsRouteHandler> _logger;
    private readonly string _username;

    public GetRulesetsRouteHandler(RulesetService rulesetService, AppUser user, ILogger<GetRulesetsRouteHandler> logger)
    {
        _rulesetService = rulesetService;
        _logger = logger;
        _username = user.Name;
    }
    
    public async Task<IResult> GetRulesetsAsync()
    {
        _logger.LogInformation("Loading rulesets for {Username}", _username);
        
        IEnumerable<Ruleset> rulesets = await _rulesetService.LoadRulesetsAsync(_username);

        return Results.Ok(rulesets);
    }
}