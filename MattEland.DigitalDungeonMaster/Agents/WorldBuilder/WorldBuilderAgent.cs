using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Plugins;
using MattEland.DigitalDungeonMaster.Blocks;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Agents.WorldBuilder;

public sealed class WorldBuilderAgent : IChatAgent
{
    private readonly RequestContextService _context;
    private readonly Kernel _kernel;
    private readonly ILogger<WorldBuilderAgent> _logger;
    private ChatHistory? _history;
    private SettingCreationPlugin? _settingPlugin;

    public WorldBuilderAgent(
        Kernel kernel,
        ILoggerFactory logFactory,
        RequestContextService context)
    {
        _context = context;
        _kernel = kernel.Clone();
        _logger = logFactory.CreateLogger<WorldBuilderAgent>();
    }

    public string Name => "World Builder";
    public bool HasCreatedWorld => _settingPlugin != null && _settingPlugin.IsFinalized;

    public async Task<ChatResult> InitializeAsync(IServiceProvider services)
    {
        // Register plugins
        _settingPlugin = new SettingCreationPlugin();
        _kernel.Plugins.AddFromObject(_settingPlugin);
        
        // Set up the history
        _history = new ChatHistory();
        const string sysPrompt = """
                                 You are a world building AI agent designed to capture details of the game world the 
                                 player of a table top role playing game wants to play in. Your job is to capture basic information
                                 from the player about the world they want to play in and then provide that information to the
                                 appropriate plugins. Feel free to elaborate on what the player has said, ask for more details,
                                 and introduce surprises or twists to the player's world without telling them. You can call
                                 ValidateWorld() to check the status of the game world to see if it is complete and ready for play.
                                 """;
        _history.AddSystemMessage(sysPrompt);

        return await ChatAsync(new ChatRequest
        {
            Message = "Greet the player and ask them to describe the world they want to play in and the character they want to play as."
        });
    }
    
    public NewGameSettingInfo? SettingInfo => _settingPlugin?.GetCurrentSettingInfo();
    
    public async Task<ChatResult> ChatAsync(ChatRequest request)
    {
        _context.BeginNewRequest(request);
        _history!.AddUserMessage(request.Message);
        
        string response = await _kernel.SendKernelMessageAsync(request, _logger, _history, Name, _context.CurrentUser!);
        
        _context.AddBlock(new MessageBlock
        {
            Message = response,
            IsUserMessage = false
        });
        
        return new ChatResult
        {
            Message = response,
            Blocks = _context.Blocks
        };
    }
}