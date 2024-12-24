using MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster;

public sealed class GameMasterAgent : AgentBase<GameChatRequest, GameChatResult>
{
    private readonly Kernel _kernel;
    private ChatHistory? _history;
    private string _name = "Game Master";

    public GameMasterAgent(Kernel kernel, ILogger<GameMasterAgent> logger) : base(logger)
    {
        _kernel = kernel.Clone();
    }
    
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
        _kernel.Plugins.AddFromType<ClassesPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<GameInfoPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<ImageGenerationPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<LocationPlugin>(serviceProvider: services);
        //_kernel.Plugins.AddFromType<SessionHistoryPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<SkillsPlugin>(serviceProvider: services);
        //_kernel.Plugins.AddFromType<StandardPromptsPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<StorytellerPlugin>(serviceProvider: services);
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
            Replies = [
                new ChatMessage
                {
                    Author = Name,
                    Message = response
                }
            ]
        };
    }

    public override string Name => _name;
}