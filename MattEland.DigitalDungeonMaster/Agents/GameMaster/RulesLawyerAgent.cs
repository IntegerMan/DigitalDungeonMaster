using MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster;

public sealed class RulesLawyerAgent(Kernel kernel, ILogger<RulesLawyerAgent> logger)
    : AgentBase<GameChatRequest, GameChatResult>(logger)
{
    private readonly Kernel _kernel = kernel.Clone();
    private ChatHistory? _history;
    
    private string _name = "Rules Lawyer";
    public override string Name => _name;

    public override void Initialize(IServiceProvider services, AgentConfig config)
    {
        // Set up the prompt
        string mainPrompt = config.FullPrompt;
        _name = config.Name;

        _history = new ChatHistory();
        Logger.LogDebug("Initializing {AgentName} with system prompt: {Prompt}", Name, mainPrompt);
        _history.AddSystemMessage(mainPrompt);

        // Add Plugins
        _kernel.Plugins.AddFromType<AttributesPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<SkillsPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<ClassesPlugin>(serviceProvider: services);
    }

    public override async Task<GameChatResult> ChatAsync(GameChatRequest request, string username)
    {
        Logger.LogInformation("{User} to {Bot}: {Message}", username, Name, request.Message);
        CopyRequestHistory(request, _history!);

        string response = await _kernel.SendKernelMessageAsync(request, Logger, _history!, Name, username);

        // If we wanted to reuse things, we'd want to stick the new history in the chat history object, but it's safer to reconstruct every request

        // Wrap everything up in a bow
        return new GameChatResult
        {
            Id = request.Id ?? Guid.NewGuid(),
            Replies =
            [
                new ChatMessage()
                {
                    Author = Name,
                    Message = response.Trim(),
                    ImageUrl = null, // TODO: Revisit this
                }
            ]
        };
    }

}