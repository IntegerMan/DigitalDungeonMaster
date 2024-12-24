using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
public class SessionHistoryPlugin : PluginBase
{
    private readonly RequestContextService _context;
    private readonly IFileStorageService _fileStorage;

    public SessionHistoryPlugin(RequestContextService context, IFileStorageService fileStorage, ILogger<SessionHistoryPlugin> logger) 
        : base(logger)
    {
        _context = context;
        _fileStorage = fileStorage;
    }
    
    [KernelFunction("GetLastSessionRecap")]
    [Description("Gets a short recap of our last adventuring session")]
    [return: Description("A short recap of the last adventuring session")]
    public async Task<string> GetLastSessionRecap()
    {
        string user = _context.CurrentUser!;
        string adventure = _context.CurrentAdventure!.RowKey;
        using Activity? activity = LogActivity($"User: {user}, Adventure: {adventure}");
        
        string? recap = await _fileStorage.LoadTextOrDefaultAsync("adventures", $"{user}_{adventure}/Recap.md");
        
        if (string.IsNullOrWhiteSpace(recap))
        {
            Logger.LogWarning("No recap was found for the last session");
            recap = "No recap was found for the last session. This may be the start of a new adventure!";
        }
        else
        {
            Logger.LogTrace("Session recap loaded: {Recap}", recap);
        }
        
        activity?.AddTag("Recap", recap);
        
        return recap;
    }
}