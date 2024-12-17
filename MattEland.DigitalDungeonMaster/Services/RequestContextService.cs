using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.Services;

public class RequestContextService
{
    private readonly ILogger<RequestContextService> _logger;
    private AdventureInfo? _currentAdventure;

    public RequestContextService(ILogger<RequestContextService> logger)
    {
        _logger = logger;
    }

    public string? CurrentRuleset => CurrentAdventure?.Ruleset;

    public AdventureInfo? CurrentAdventure
    {
        get => _currentAdventure;
        set
        {
            if (_currentAdventure == value) return;
            _logger.LogTrace("Setting current adventure to {Adventure}", value);
            _currentAdventure = value;
        }
    }
    
    public string? CurrentUser { get; set; }
}