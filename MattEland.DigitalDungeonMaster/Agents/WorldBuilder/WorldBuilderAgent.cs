using MattEland.DigitalDungeonMaster.Agents.GameMaster;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Plugins;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.DependencyInjection;
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

    public override string Name { get; protected set; } = "World Builder";
    public override void Initialize(IServiceProvider services, AgentConfig config)
    {
        // Register plugins
        _settingPlugin = new SettingCreationPlugin(services.GetRequiredService<ILogger<SettingCreationPlugin>>());
        _kernel.Plugins.AddFromObject(_settingPlugin);

        // Set up the history
        _history = new ChatHistory();
        _history.AddSystemMessage(config.FullPrompt);
    }

    public override async Task<WorldBuilderChatResult> ChatAsync(WorldBuilderChatRequest request, string username, CancellationToken token = default)
    {
        _settingPlugin!.SettingInfo = request.Data;

        Logger.LogInformation("{User} to {Bot}: {Message}", username, Name, request.Message);
        CopyRequestHistory(request, _history!);

        string response = await _kernel.SendKernelMessageAsync(request, Logger, _history!, Name, username, token);

        return new WorldBuilderChatResult
        {
            Id = request.Id ?? Guid.NewGuid(),
            Data = _settingPlugin!.GetCurrentSettingInfo(),
            Replies =
            [
                new ChatMessage
                {
                    Author = Name,
                    Message = response.Trim()
                }
            ]
        };
    }
}