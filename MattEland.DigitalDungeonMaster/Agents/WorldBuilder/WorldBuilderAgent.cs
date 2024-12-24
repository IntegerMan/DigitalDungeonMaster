using MattEland.DigitalDungeonMaster.Agents.GameMaster;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Plugins;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Agents.WorldBuilder;

public sealed class WorldBuilderAgent : AgentBase<WorldBuilderChatRequest, WorldBuilderChatResult>
{
    private readonly Kernel _kernel;
    private ChatHistory? _history;
    private SettingCreationPlugin? _settingPlugin;

    public WorldBuilderAgent(Kernel kernel, ILogger<WorldBuilderAgent> logger) : base(logger)
    {
        _kernel = kernel.Clone();
    }

    public override string Name => "World Builder";
    public bool HasCreatedWorld => _settingPlugin is { IsFinalized: true };

    public SettingCreationPlugin SettingPlugin => _settingPlugin ?? throw new InvalidOperationException("Setting plugin not initialized");

    public override void Initialize(IServiceProvider services, AgentConfig config)
    {
        // Register plugins
        _settingPlugin = new SettingCreationPlugin();
        _kernel.Plugins.AddFromObject(_settingPlugin);
        
        // Set up the history
        _history = new ChatHistory();
        _history.AddSystemMessage(config.FullPrompt);
    }
    
    public override async Task<WorldBuilderChatResult> ChatAsync(WorldBuilderChatRequest request, string username)
    {
        _settingPlugin!.SettingInfo = request.Data;
        
        Logger.LogInformation("{User} to {Bot}: {Message}", username, Name, request.Message);
        CopyRequestHistory(request, _history!);

        string response = await _kernel.SendKernelMessageAsync(request, Logger, _history!, Name, username);
        
        return new WorldBuilderChatResult
        {
            Id = request.Id ?? Guid.NewGuid(),
            Data = _settingPlugin!.GetCurrentSettingInfo(),
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