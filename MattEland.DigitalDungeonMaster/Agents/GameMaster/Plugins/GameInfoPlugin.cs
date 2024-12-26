using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated via Dependency Injection")]
[Description("Provides information on the current game")]
public class GameInfoPlugin(
    RequestContextService context,
    IFileStorageService storageService,
    ILogger<GameInfoPlugin> logger)
    : PluginBase(logger)
{
    [KernelFunction("GetPlayerInformation")]
    [Description("Gets information on the player character or characters in the play session")]
    [return: Description("Information on the player characters")]
    public async Task<string> GetPlayerCharacters()
    {
        using Activity? activity = LogActivity($"User: {context.CurrentUser}, Adventure: {context.CurrentAdventure!.RowKey}");
        string key = $"{context.CurrentUser}_{context.CurrentAdventure!.RowKey}/Players.md";

        string result = await storageService.LoadTextOrDefaultAsync("adventures", key) ?? "No information is available on the player character";
        activity?.AddTag("Result", result);
        
        return result;
    }
}