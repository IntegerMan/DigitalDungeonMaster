using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
public class SessionHistoryPlugin : BasiliskPlugin
{
    private readonly StorageDataService _storageService;

    public SessionHistoryPlugin(RequestContextService context, StorageDataService storageService) : base(context)
    {
        _storageService = storageService;
    }
    
    [KernelFunction("GetLastSessionRecap")]
    [Description("Gets a short recap of our last adventuring session")]
    [return: Description("A short recap of the last adventuring session")]
    public async Task<string> GetLastSessionRecap()
    {
        string user = Context.CurrentUser!;
        string adventure = Context.CurrentAdventureId!;
        Context.LogPluginCall($"User: {user}, Adventure: {adventure}");
        
        string recap = await _storageService.LoadTextAsync("adventures", $"{user}_{adventure}/Recap.md");
        
        if (string.IsNullOrWhiteSpace(recap))
        {
            recap = "No recap was found for the last session. This may be the start of a new adventure!";
        }
        
        Context.AddBlock(new TextResourceBlock("Session Recap", recap));
        
        return recap;
    }
}