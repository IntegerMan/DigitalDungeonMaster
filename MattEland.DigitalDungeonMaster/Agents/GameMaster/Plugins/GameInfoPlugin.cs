using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("Provides information on the current game")]
public class GameInfoPlugin
{
    private readonly RequestContextService _context;
    private readonly IStorageService _storageService;
    private readonly ILogger<GameInfoPlugin> _logger;

    public GameInfoPlugin(RequestContextService context, IStorageService storageService, ILogger<GameInfoPlugin> logger)
    {
        _context = context;
        _storageService = storageService;
        _logger = logger;
    }
    
    [KernelFunction("GetPlayerInformation")]
    [Description("Gets information on the player character or characters in the play session")]
    [return: Description("Information on the player characters")]
    public async Task<string> GetPlayerCharacters()
    {
        _logger.LogDebug("{Plugin}-{Method} called for user {User} and adventure {Adventure}", nameof(GameInfoPlugin), nameof(GetPlayerCharacters), _context.CurrentUser, _context.CurrentAdventure.RowKey);
        string key = $"{_context.CurrentUser}_{_context.CurrentAdventure!.RowKey}/Players.md";
        
        return await _storageService.LoadTextAsync("adventures", key) ?? "No information is available on the player character";
    }
}