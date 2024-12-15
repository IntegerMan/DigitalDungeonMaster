using MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;
using MattEland.DigitalDungeonMaster.Blocks;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster;

public sealed class GameMasterAgent : IChatAgent
{
    private readonly Kernel _kernel;
    private readonly RequestContextService _context;
    private readonly ILogger<GameMasterAgent> _logger;

    public GameMasterAgent(
        Kernel kernel,
        RequestContextService contextService,
        ILoggerFactory logFactory)
    {
        _kernel = kernel.Clone();
        _context = contextService;
        _logger = logFactory.CreateLogger<GameMasterAgent>();
    }

    public bool IsNewAdventure { get; set; } = true;
    
    public string? AdditionalPrompt { get; set; }
    
    public async Task<ChatResult> InitializeAsync(IServiceProvider services)
    {
        AgentConfigurationService agentService = services.GetRequiredService<AgentConfigurationService>();
        AgentConfig config = agentService.GetAgentConfiguration("DM");

        // Set up the prompt
        string mainPrompt = config.MainPrompt;
        if (!string.IsNullOrWhiteSpace(AdditionalPrompt))
        {
            mainPrompt += $"\n\n{AdditionalPrompt}";
        }
        _context.History.AddSystemMessage(mainPrompt);

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
        return IsNewAdventure switch
        {
            true when !string.IsNullOrWhiteSpace(config.NewCampaignPrompt) => await ChatAsync(new ChatRequest
                {
                    Message = config.NewCampaignPrompt
                }),
            
            false when !string.IsNullOrWhiteSpace(config.ResumeCampaignPrompt) => await ChatAsync(
                new ChatRequest
                {
                    Message = config.ResumeCampaignPrompt
                }),
            
            _ => new ChatResult()
            {
                Id = Guid.NewGuid(),
                Replies = [
                    new ChatMessage
                    {
                        Author = "Game Master",
                        Message = "Welcome to the game"
                    }
                ]
            }
        };
    }

    public async Task<ChatResult> ChatAsync(ChatRequest request)
    {
        _context.BeginNewRequest(request);
        
        string response = await _kernel.SendKernelMessageAsync(request, _logger, _context.History, Name, _context.CurrentUser!);
                
        // Add the response to the displayable results
        _context.AddBlock(new MessageBlock
        {
            Message = response,
            IsUserMessage = false
        });

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