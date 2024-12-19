using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Plugins;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Agents.WorldBuilder;

public sealed class WorldBuilderAgent : IChatAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger<WorldBuilderAgent> _logger;
    private ChatHistory? _history;
    private SettingCreationPlugin? _settingPlugin;

    public WorldBuilderAgent(
        Kernel kernel,
        ILoggerFactory logFactory)
    {
        _kernel = kernel.Clone();
        _logger = logFactory.CreateLogger<WorldBuilderAgent>();
    }

    public string Name => "World Builder";
    public bool HasCreatedWorld => _settingPlugin is { IsFinalized: true };

    public SettingCreationPlugin SettingPlugin => _settingPlugin ?? throw new InvalidOperationException("Setting plugin not initialized");
    
    public void Initialize(IServiceProvider services, AgentConfig config)
    {
        // Register plugins
        _settingPlugin = new SettingCreationPlugin();
        _kernel.Plugins.AddFromObject(_settingPlugin);
        
        // Set up the history
        _history = new ChatHistory();
        _history.AddSystemMessage(config.FullPrompt);
    }
    
    public NewGameSettingInfo? SettingInfo => _settingPlugin?.GetCurrentSettingInfo();
    
    public async Task<ChatResult> ChatAsync(ChatRequest request, string username)
    {
        _history!.AddUserMessage(request.Message);
        
        string response = await _kernel.SendKernelMessageAsync(request, _logger, _history, Name, username);
        
        return new ChatResult
        {
            Id = request.Id ?? Guid.NewGuid(),
            Replies = [
                new ChatMessage
                {
                    Author = Name,
                    Message = response
                }
            ]
        };
    }
}