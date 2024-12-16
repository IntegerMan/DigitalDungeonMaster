using MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster;

public sealed class GameMasterAgent : IChatAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger<GameMasterAgent> _logger;
    private ChatHistory _history;

    public GameMasterAgent(
        Kernel kernel,
        ILoggerFactory logFactory)
    {
        _kernel = kernel.Clone();
        _logger = logFactory.CreateLogger<GameMasterAgent>();
    }
    
    public void Initialize(IServiceProvider services, AgentConfig config)
    {
        // Set up the prompt
        string mainPrompt = config.FullPrompt;
        Name = config.Name;

        _history = new ChatHistory();
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

    public async Task<ChatResult> ChatAsync(ChatRequest request, string username)
    {
        _logger.LogInformation("{User} to {Bot}: {Message}", username, Name, request.Message);
        
        // TODO: Add history from the request
        
        string response = await _kernel.SendKernelMessageAsync(request, _logger, _history, Name, username);

        // Wrap everything up in a bow
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

    public string Name { get; set; } = "Game Master";
}