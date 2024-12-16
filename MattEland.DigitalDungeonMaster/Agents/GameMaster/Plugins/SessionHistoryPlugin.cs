using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
public class SessionHistoryPlugin
{
    private readonly RequestContextService _context;
    private readonly IStorageService _storageService;
    private readonly ILogger<SessionHistoryPlugin> _logger;

    public SessionHistoryPlugin(RequestContextService context, IStorageService storageService, ILogger<SessionHistoryPlugin> logger) 
    {
        _context = context;
        _storageService = storageService;
        _logger = logger;
    }
    
    [KernelFunction("GetLastSessionRecap")]
    [Description("Gets a short recap of our last adventuring session")]
    [return: Description("A short recap of the last adventuring session")]
    public async Task<string> GetLastSessionRecap()
    {
        string user = _context.CurrentUser!;
        string adventure = _context.CurrentAdventure!.RowKey;
        _logger.LogDebug("{Plugin}-{Method} called for user {User} and adventure {Adventure}", nameof(SessionHistoryPlugin), nameof(GetLastSessionRecap), user, adventure);
        
        string? recap = await _storageService.LoadTextOrDefaultAsync("adventures", $"{user}_{adventure}/Recap.md");
        
        if (string.IsNullOrWhiteSpace(recap))
        {
            _logger.LogWarning("No recap was found for the last session");
            recap = "No recap was found for the last session. This may be the start of a new adventure!";
        }
        else
        {
            _logger.LogTrace("Session recap loaded: {Recap}", recap);
        }
        
        return recap;
    }
}