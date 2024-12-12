using System.ClientModel;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;
using MattEland.DigitalDungeonMaster.Blocks;
using MattEland.DigitalDungeonMaster.Models;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

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
    
    public async Task<ChatResult> InitializeAsync(IServiceProvider services)
    {
        AgentConfigurationService agentService = services.GetRequiredService<AgentConfigurationService>();
        AgentConfig config = agentService.GetAgentConfiguration("DM");
        
        _context.History.AddSystemMessage(config.MainPrompt);

        // Add Plugins
        _kernel.Plugins.AddFromType<AttributesPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<ClassesPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<GameInfoPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<ImageGenerationPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<LocationPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<SessionHistoryPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<SkillsPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<StandardPromptsPlugin>(serviceProvider: services);
        _kernel.Plugins.AddFromType<StorytellerPlugin>(serviceProvider: services);

        // Make the initial request
        return IsNewAdventure switch
        {
            true when !string.IsNullOrWhiteSpace(config.NewCampaignPrompt) => await ChatAsync(new ChatRequest()
                {
                    Message = config.NewCampaignPrompt
                }),
            
            false when !string.IsNullOrWhiteSpace(config.ResumeCampaignPrompt) => await ChatAsync(
                new ChatRequest
                {
                    Message = config.ResumeCampaignPrompt,
                    ClearFirst = false
                }),
            
            _ => new ChatResult { Message = "The game is ready to begin", Blocks = _context.Blocks }
        };
    }

    public async Task<ChatResult> ChatAsync(ChatRequest request)
    {
        _logger.LogDebug("{Agent}: {Message}", "User", request.Message);
        _context.BeginNewRequest(request);
        _context.History.AddUserMessage(request.Message); // TODO: We may need to move to a sliding window history approach
        
        // Set up settings
        OpenAIPromptExecutionSettings settings = new()
        {
            User = $"MattEland.DigitalDungeonMaster User: {_context.CurrentUser}",
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true, options: new FunctionChoiceBehaviorOptions
            {
                AllowConcurrentInvocation = true,
                AllowParallelCalls = null
            }),
        };

        string? response;
        try
        {
            IChatCompletionService chat = _kernel.GetRequiredService<IChatCompletionService>();
            ChatMessageContent result = await chat.GetChatMessageContentAsync(_context.History, settings, _kernel);
            _context.History.Add(result);
            
            response = result.Content;
            _logger.LogDebug("{Agent}: {Message}", Name, response);

        }
        catch (Exception ex) when (ex is ClientResultException or HttpOperationException)
        {
            _logger.LogError(ex, "{Type} Error: {Message}", ex.GetType().FullName, ex.Message);
            
            if (ex.Message.Contains("content management", StringComparison.OrdinalIgnoreCase)) 
            {
                response = "I'm afraid that message is a bit too spicy for what I'm allowed to process. Can you try something else?";
            }            
            else if (ex.Message.Contains("429", StringComparison.OrdinalIgnoreCase)) 
            {
                response = "I'm a bit overloaded at the moment. Please wait a minute and try again.";
            }            
            else if (ex.Message.Contains("server_error", StringComparison.OrdinalIgnoreCase)) 
            {
                response = "There was an error with the large language model that hosts my brain. Please try again later.";
            }
            else
            {
                response = "I couldn't handle your request due to an error. Please try again later or report this issue if it persists.";
            }
        }
        
        response ??= "I'm afraid I can't respond to that right now";
        
        // Add the response to the displayable results
        _context.AddBlock(new MessageBlock
        {
            Message = response,
            IsUserMessage = false
        });

        // Wrap everything up in a bow
        return new ChatResult
        {
            Message = response,
            Blocks = _context.Blocks,
            // TODO: It'd be nice to include token usage metrics here
        };
    }

    public string Name => "Game Master";
}