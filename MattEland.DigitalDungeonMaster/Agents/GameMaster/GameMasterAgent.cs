using MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster;

public sealed class GameMasterAgent : IChatAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger<GameMasterAgent> _logger;
    private ChatHistory? _history;

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
        _logger.LogDebug("Initializing {AgentName} with system prompt: {Prompt}", Name, mainPrompt);
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

    public async Task<IChatResult> ChatAsync(IChatRequest request, string username)
    {
        _logger.LogInformation("{User} to {Bot}: {Message}", username, Name, request.Message);

        if (request.History != null)
        {
            foreach (var entry in request.History)
            {
                _logger.LogTrace("{Author}: {Message}", entry.Author, entry.Message);

                if (!string.IsNullOrWhiteSpace(entry.Message))
                {
                    if (entry.Author == request.User)
                    {
                        _history!.AddUserMessage(entry.Message!);
                    }
                    else
                    {
                        _history!.AddAssistantMessage(entry.Message!);
                    }
                }
            }
        }

        string response = await _kernel.SendKernelMessageAsync(request, _logger, _history!, Name, username);
        
        // If we wanted to reuse things, we'd want to stick the new history in the chat history object, but it's safer to reconstruct every request

        // Wrap everything up in a bow
        return new ChatResult<object>
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