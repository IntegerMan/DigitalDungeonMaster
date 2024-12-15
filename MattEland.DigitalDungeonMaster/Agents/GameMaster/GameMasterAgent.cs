using MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster;

public sealed class GameMasterAgent : IChatAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger<GameMasterAgent> _logger;

    public GameMasterAgent(
        Kernel kernel,
        ILoggerFactory logFactory)
    {
        _kernel = kernel.Clone();
        _logger = logFactory.CreateLogger<GameMasterAgent>();
    }

    public bool IsNewAdventure { get; set; } = true;
    
    public string? AdditionalPrompt { get; set; }
    
    public async Task<ChatResult> InitializeAsync(IServiceProvider services, string username)
    {
        AgentConfigurationService agentService = services.GetRequiredService<AgentConfigurationService>();
        AgentConfig config = agentService.GetAgentConfiguration("GM");

        // Set up the prompt
        string mainPrompt = config.MainPrompt;
        if (!string.IsNullOrWhiteSpace(AdditionalPrompt))
        {
            mainPrompt += $"\n\n{AdditionalPrompt}";
        }

        ChatHistory history = new();
        history.AddSystemMessage(mainPrompt);

        // Add Plugins
        _kernel.Plugins.AddFromType<AttributesPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<ClassesPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<GameInfoPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<ImageGenerationPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<LocationPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<SessionHistoryPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<SkillsPlugin>(serviceProvider: services);
        //_kernel.Plugins.AddFromType<StandardPromptsPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<StorytellerPlugin>(serviceProvider: services);

        // Make the initial request
        ChatRequest request = new ChatRequest
        {
            RecipientName = config.Name,
            Message = IsNewAdventure switch
            {
                true => config.NewCampaignPrompt ?? throw new InvalidOperationException("No new campaign prompt found"),
                false => config.ResumeCampaignPrompt ?? throw new InvalidOperationException("No resume campaign prompt found")
            }
        };
        return await ChatAsync(request, username);
    }

    public async Task<ChatResult> ChatAsync(ChatRequest request, string username)
    {
        ChatHistory history = new();
        // TODO: Add history from the request
        
        string response = await _kernel.SendKernelMessageAsync(request, _logger, history, Name, username);
                
        // Add the response to the displayable results
        /*
        _context.AddBlock(new MessageBlock
        {
            Message = response,
            IsUserMessage = false
        });
        */

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

    public string Name => "Game Master";
}