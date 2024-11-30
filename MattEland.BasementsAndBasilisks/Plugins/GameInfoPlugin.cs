using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "GameInformation")]
public class GameInfoPlugin
{
    private readonly RequestContextService _context;
    private readonly StorageDataService _storageService;

    public GameInfoPlugin(RequestContextService context, StorageDataService storageService)
    {
        _context = context;
        _storageService = storageService;
    }
    
    [KernelFunction("GetPlayerInformation")]
    [Description("Gets information on the player character or characters in the play session")]
    [return: Description("Information on the player characters")]
    public async Task<string> GetPlayerCharacters()
    {
        string key = $"{_context.CurrentUser}_{_context.CurrentAdventureId}/Players.md";
        _context.LogPluginCall(key);

        return await _storageService.LoadTextAsync("adventures", key) ?? "No information is available on the player character";
    }
}