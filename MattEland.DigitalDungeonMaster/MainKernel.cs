using System.ClientModel;
using MattEland.DigitalDungeonMaster.Blocks;
using MattEland.DigitalDungeonMaster.Models;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel.TextToImage;

#pragma warning disable SKEXP0001 // Text to image service

namespace MattEland.DigitalDungeonMaster;

public sealed class MainKernel
{
    private readonly Kernel _kernel;
    private readonly RequestContextService _context;
    private readonly ILogger<MainKernel> _logger;

    public MainKernel( 
        IChatCompletionService chatCompletionService,
        ITextToImageService textToImageService,
        ITextGenerationService textGenerationService,
        RequestContextService contextService,
        ILoggerFactory logFactory)
    {
        _context = contextService;
        _logger = logFactory.CreateLogger<MainKernel>();
        
        // Set up Semantic Kernel
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.Services.AddScoped<IChatCompletionService>(_ => chatCompletionService);
        builder.Services.AddScoped<ITextToImageService>(_ => textToImageService);
        builder.Services.AddScoped<ITextGenerationService>(_ => textGenerationService);
        builder.Services.AddScoped<ILoggerFactory>(_ => logFactory);
        
        _kernel = builder.Build();
    }

    public async Task<ChatResult> InitializeKernelAsync(IServiceProvider services, bool isNewAdventure)
    {
        AgentConfigurationService agentService = services.GetRequiredService<AgentConfigurationService>();
        AgentConfig config = agentService.GetAgentConfiguration("DM");
        AgentName = config.Name;
        
        _context.History.AddSystemMessage(config.MainPrompt);

        // Add Plugins
        _kernel.RegisterGamePlugins(services);

        // Make the initial request
        return isNewAdventure switch
        {
            true when !string.IsNullOrWhiteSpace(config.NewCampaignPrompt) => await ChatAsync(config.NewCampaignPrompt,
                clearBlocks: true),
            
            false when !string.IsNullOrWhiteSpace(config.ResumeCampaignPrompt) => await ChatAsync(
                config.ResumeCampaignPrompt, clearBlocks: false),
            
            _ => new ChatResult { Message = "The game is ready to begin", Blocks = _context.Blocks }
        };
    }

    public async Task<ChatResult> ChatAsync(string message, bool clearBlocks = true)
    {
        _logger.LogDebug("{Agent}: {Message}", "User", message);
        _context.BeginNewRequest(message, clearBlocks);
        _context.History.AddUserMessage(message); // TODO: We may need to move to a sliding window history approach
        
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
            _logger.LogDebug("{Agent}: {Message}", AgentName, response);

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

    public string AgentName { get; set; }
}