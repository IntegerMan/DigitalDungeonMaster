using System.Runtime.CompilerServices;
using MattEland.DigitalDungeonMaster.Blocks;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Services;

public class RequestContextService
{
    private readonly ILogger<RequestContextService> _logger;
    private readonly List<ChatBlockBase> _blocks = new();
    private AdventureInfo? _currentAdventure;

    public RequestContextService(ILogger<RequestContextService> logger)
    {
        _logger = logger;
    }

    public IEnumerable<ChatBlockBase> Blocks => _blocks.AsReadOnly();
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

    internal ChatHistory History { get; } = new();
    public string? CurrentUser { get; set; }

    public void BeginNewRequest(ChatRequest request)
    {
        _logger.LogDebug("Beginning new request with message: {Message}", request.Message);
        
        /* TODO: Implement this
        if (request.ClearFirst)
        {
            ClearBlocks();
        }
        */

        _blocks.Add(new MessageBlock
        {
            Message = request.Message,
            IsUserMessage = true,
        });
    }

    public void LogPluginCall(string? metadata = null, [CallerMemberName] string caller = "")
    {
        _logger.LogDebug("{Plugin} Called with Metadata: {Metadata}", caller, metadata);
    }

    public void ClearBlocks()
    {
        _logger.LogDebug("Clearing blocks");
        
        _blocks.Clear();
    }
}