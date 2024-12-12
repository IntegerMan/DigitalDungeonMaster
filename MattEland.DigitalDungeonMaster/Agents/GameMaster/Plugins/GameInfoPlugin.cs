using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("Provides information on the current game")]
public class GameInfoPlugin : GamePlugin
{
    private readonly StorageDataService _storageService;

    public GameInfoPlugin(RequestContextService context, StorageDataService storageService) : base(context)
    {
        _storageService = storageService;
    }
    
    [KernelFunction("GetPlayerInformation")]
    [Description("Gets information on the player character or characters in the play session")]
    [return: Description("Information on the player characters")]
    public async Task<string> GetPlayerCharacters()
    {
        string key = $"{Context.CurrentUser}_{Context.CurrentAdventureId}/Players.md";
        Context.LogPluginCall(key);

        return await _storageService.LoadTextAsync("adventures", key) ?? "No information is available on the player character";
    }
}