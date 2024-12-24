using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("Provides information on the current game")]
public class GameInfoPlugin : PluginBase
{
    private readonly RequestContextService _context;
    private readonly IFileStorageService _storageService;

    public GameInfoPlugin(RequestContextService context, IFileStorageService storageService, ILogger<GameInfoPlugin> logger)
        : base(logger)
    {
        _context = context;
        _storageService = storageService;
    }
    
    [KernelFunction("GetPlayerInformation")]
    [Description("Gets information on the player character or characters in the play session")]
    [return: Description("Information on the player characters")]
    public async Task<string> GetPlayerCharacters()
    {
        using Activity? activity = LogActivity($"User: {_context.CurrentUser}, Adventure: {_context.CurrentAdventure!.RowKey}");
        string key = $"{_context.CurrentUser}_{_context.CurrentAdventure!.RowKey}/Players.md";

        string result = await _storageService.LoadTextOrDefaultAsync("adventures", key) ?? "No information is available on the player character";
        activity?.AddTag("Result", result);
        
        return result;
    }
}