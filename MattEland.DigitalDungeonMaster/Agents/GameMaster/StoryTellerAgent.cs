using System.Text;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.TextGeneration;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster;

public sealed class StoryTellerAgent : IChatAgent<GameChatRequest, GameChatResult>
{
    private readonly ITextGenerationService _textService;
    private readonly ILogger<StoryTellerAgent> _logger;
    private string _systemPrompt = string.Empty;
    private readonly PromptExecutionSettings _executionSettings = new()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };
    private readonly Kernel _kernel;

    public StoryTellerAgent(Kernel kernel, ITextGenerationService textService, ILogger<StoryTellerAgent> logger)
    {
        _kernel = kernel.Clone();
        _textService = textService;
        _logger = logger;
    }

    public string Name { get; private set; } = "Story Teller";

    public void Initialize(IServiceProvider services, AgentConfig config)
    {
        Name = config.Name;
        _systemPrompt = config.FullPrompt;
        
        // Add plugins that relate to the Story Teller - TODO: This should probably come from config at some point
        _kernel.Plugins.AddFromType<StorytellerPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<LocationPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<GameInfoPlugin>(serviceProvider: services);
    }

    public async Task<GameChatResult> ChatAsync(GameChatRequest request, string username, CancellationToken token = default)
    {
        StringBuilder prompt = new(_systemPrompt);
        prompt.AppendLine();
        prompt.AppendLine("Here's the conversation thus far:");
        if (request.History != null)
        {
            foreach (var message in request.History)
            {
                prompt.AppendLine($"{message.Author}: {message.Message}");
            }
        }

        prompt.AppendLine();
        prompt.AppendLine("The user just typed:");
        prompt.AppendLine($"{username}: {request.Message}");
        prompt.AppendLine();
        prompt.AppendLine("Taking this into account, give a succinct single recommendation to the Game Master on how to handle this message.");
        
        string finalPrompt = prompt.ToString();
        
        _logger.LogDebug("Responding to prompt: {Prompt}", finalPrompt);
        
        IReadOnlyList<TextContent> result = await _textService.GetTextContentsAsync(finalPrompt, _executionSettings, _kernel, token); 

        return new GameChatResult
        {
            Id = request.Id ?? Guid.NewGuid(),
            Replies = result.Where(r => !string.IsNullOrWhiteSpace(r.Text))
                .Select(r => new ChatMessage
                {
                    Author = Name,
                    Message = r.Text
                }).ToList()
        };
    }
}