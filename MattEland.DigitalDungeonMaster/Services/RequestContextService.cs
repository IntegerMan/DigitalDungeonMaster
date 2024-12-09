using System.Runtime.CompilerServices;
using MattEland.DigitalDungeonMaster.Blocks;
using MattEland.DigitalDungeonMaster.Models;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Services;

public class RequestContextService
{
    private readonly ILogger<RequestContextService> _logger;
    private readonly List<ChatBlockBase> _blocks = new();
    private AdventureInfo? _currentAdventure;
    private string? _currentUser;

    public RequestContextService(ILogger<RequestContextService> logger)
    {
        _logger = logger;
    }
    
    public void AddBlock(ChatBlockBase block)
    {
        _blocks.Add(block);
    }

    public IEnumerable<ChatBlockBase> Blocks => _blocks.AsReadOnly();
    public string? CurrentRuleset => CurrentAdventure?.Ruleset;

    public string? CurrentUser
    {
        get => _currentUser;
        set
        {
            if (_currentUser == value) return;
            _logger.LogTrace("Setting current user to {User}", value);
            _currentUser = value;
        }
    }

    public string? CurrentAdventureId => CurrentAdventure?.RowKey;

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

    public void BeginNewRequest(string userMessage, bool clear)
    {
        _logger.LogDebug("Beginning new request with message: {Message}", userMessage);
        
        if (clear)
        {
            ClearBlocks();
        }

        _blocks.Add(new MessageBlock
        {
            Message = userMessage,
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

    public void Logout()
    {
        _logger.LogInformation("Logging out user {User}", CurrentUser);
        
        CurrentUser = null;
        CurrentAdventure = null;
    }
}