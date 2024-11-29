using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "SessionHistory")]
public class SessionHistoryPlugin
{
    private readonly RequestContextService _context;
    private readonly StorageDataService _storageService;

    public SessionHistoryPlugin(RequestContextService context, StorageDataService storageService)
    {
        _context = context;
        _storageService = storageService;
    }
    
    [KernelFunction("GetLastSessionRecap")]
    [Description("Gets a short recap of our last adventuring session")]
    [return: Description("A short recap of the last adventuring session")]
    public async Task<string> GetLastSessionRecap()
    {
        string user = _context.CurrentUser;
        string adventure = _context.CurrentAdventureName;
        _context.LogPluginCall($"User: {user}, Adventure: {adventure}");
        
        string recap = await _storageService.LoadTextAsync("adventures", $"{user}_{adventure}/Recap.md");
        
        if (string.IsNullOrWhiteSpace(recap))
        {
            recap = "No recap was found for the last session. This may be the start of a new adventure!";
        }
        
        _context.AddBlock(new TextResourceBlock("Session Recap", recap));
        
        return recap;
    }
}