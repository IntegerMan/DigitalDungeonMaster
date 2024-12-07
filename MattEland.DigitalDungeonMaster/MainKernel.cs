using System.ClientModel;
using MattEland.DigitalDungeonMaster.Blocks;
using MattEland.DigitalDungeonMaster.Models;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel.TextToImage;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;

#pragma warning disable SKEXP0001 // Text to Image
#pragma warning disable SKEXP0010 // Text to Image and Text Embedding

namespace MattEland.DigitalDungeonMaster;

public sealed class MainKernel : IDisposable
{
    private readonly Kernel _kernel;
    private RequestContextService? _context;
    private bool _disposedValue;

    private readonly Logger _logger;

    private readonly OpenAIPromptExecutionSettings _executionSettings;
    private readonly ChatHistory _history;

    public MainKernel(IServiceCollection services, 
        AzureResourceConfig config, 
        string logPath)
    {
        // Set up persistent resources
        _history = new ChatHistory();
        _executionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true, options: new FunctionChoiceBehaviorOptions
            {
                AllowConcurrentInvocation = true,
                AllowParallelCalls = null
            }),
        };
        
        // Set up Semantic Kernel
        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config.AzureOpenAiChatDeploymentName,
                config.AzureOpenAiEndpoint,
                config.AzureOpenAiKey)
            .AddAzureOpenAITextEmbeddingGeneration(config.AzureOpenAiEmbeddingDeploymentName,
                config.AzureOpenAiEndpoint,
                config.AzureOpenAiKey)
            .AddAzureOpenAITextToImage(config.AzureOpenAiImageDeploymentName,
                config.AzureOpenAiEndpoint,
                config.AzureOpenAiKey);
        
        builder.Services.AddLogging(s => s.AddSerilog(_logger, dispose: true));
        _kernel = builder.Build();

        // Set up services
        services.AddScoped<IChatCompletionService>(_ => _kernel.GetRequiredService<IChatCompletionService>());
        services.AddScoped<ITextToImageService>(_ => _kernel.GetRequiredService<ITextToImageService>());
        services.AddScoped<ITextGenerationService>(_ => _kernel.GetRequiredService<ITextGenerationService>());
        
        // Set up logging
        _logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(new CompactJsonFormatter(), path: logPath)
            .CreateLogger();
    }

    public async Task<ChatResult> InitializeKernelAsync(IServiceProvider services, bool isNewAdventure)
    {
        // Pull from service provider
        _context = services.GetRequiredService<RequestContextService>();

        AgentConfigurationService agentService = services.GetRequiredService<AgentConfigurationService>();
        AgentConfig config = agentService.GetAgentConfiguration("DM");

        _history.AddSystemMessage(config.MainPrompt);

        // Add Plugins
        _kernel.RegisterGamePlugins(services);

        // Make the initial request
        return isNewAdventure switch
        {
            true when !string.IsNullOrWhiteSpace(config.NewCampaignPrompt) => await ChatAsync(config.NewCampaignPrompt,
                clearHistory: true),
            false when !string.IsNullOrWhiteSpace(config.ResumeCampaignPrompt) => await ChatAsync(
                config.ResumeCampaignPrompt, clearHistory: false),
            _ => new ChatResult { Message = "The game is ready to begin", Blocks = _context.Blocks }
        };
    }

    public async Task<ChatResult> ChatAsync(string message, bool clearHistory = true)
    {
        _logger.Information("{Agent}: {Message}", "User", message);
        _history.AddUserMessage(message); // TODO: We may need to move to a sliding window history approach
        _context!.BeginNewRequest(message, clearHistory);

        if (_kernel == null)
        {
            throw new InvalidOperationException("The kernel has not been initialized");
        }

        string? response;
        try
        {
            IChatCompletionService chat = _kernel.GetRequiredService<IChatCompletionService>();
            ChatMessageContent result = await chat.GetChatMessageContentAsync(_history, _executionSettings, _kernel);
            _history.Add(result);

            _logger.Information("{Agent}: {Message}", "User", message);

            response = result.Content;
        }
        catch (Exception ex) when (ex is ClientResultException or HttpOperationException)
        {
            _logger.Error(ex, "{Type} Error: {Message}", ex.GetType().FullName, ex.Message);
            
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

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _logger.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
    }
}