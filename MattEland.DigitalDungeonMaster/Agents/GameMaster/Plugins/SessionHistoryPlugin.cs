using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
public class SessionHistoryPlugin : GamePlugin
{
    private readonly IStorageService _storageService;
    private readonly ILogger<SessionHistoryPlugin> _logger;

    public SessionHistoryPlugin(RequestContextService context, IStorageService storageService, ILogger<SessionHistoryPlugin> logger) 
        : base(context)
    {
        _storageService = storageService;
        _logger = logger;
    }
    
    [KernelFunction("GetLastSessionRecap")]
    [Description("Gets a short recap of our last adventuring session")]
    [return: Description("A short recap of the last adventuring session")]
    public async Task<string> GetLastSessionRecap()
    {
        string user = Context.CurrentUser!;
        string adventure = Context.CurrentAdventureId!;
        Context.LogPluginCall($"User: {user}, Adventure: {adventure}");
        
        string? recap = await _storageService.LoadTextOrDefaultAsync("adventures", $"{user}_{adventure}/Recap.md");
        
        if (string.IsNullOrWhiteSpace(recap))
        {
            _logger.LogWarning("No recap was found for the last session");
            recap = "No recap was found for the last session. This may be the start of a new adventure!";
        }
        else
        {
            _logger.LogInformation("Session recap loaded: {Recap}", recap);
        }
        
        return recap;
    }
}