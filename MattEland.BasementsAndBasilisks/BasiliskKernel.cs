using System.ClientModel;
using System.Text.Json;
using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Core;

namespace MattEland.BasementsAndBasilisks;

public sealed class BasiliskKernel : IDisposable
{
    private Kernel? _kernel;
    private IChatCompletionService? _chat;
    private bool _disposedValue;

    private readonly Logger _logger;
    private readonly RequestContextService _context;
    private readonly StorageDataService _storage;
    private readonly OpenAIPromptExecutionSettings _executionSettings;
    private readonly ChatHistory _history;
    private readonly IServiceProvider _services;
    private readonly BasiliskConfig _config;

    public BasiliskKernel(IServiceProvider services, 
        BasiliskConfig config, 
        string logPath)
    {
        // Get necessary services
        _services = services;
        _config = config;
        _context = services.GetRequiredService<RequestContextService>();
        _storage = services.GetRequiredService<StorageDataService>();
        
        // Set up persistent resources
        _history = new ChatHistory();
        _executionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true)
        };
        
        // Set up logging
        _logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(new CompactJsonFormatter(), path: logPath)
            .CreateLogger();
    }

    public async Task<ChatResult> InitializeKernelAsync()
    {
        // Load our resources
        string key = $"{_context.CurrentUser}_{_context.CurrentAdventureId}/gameconfig.json";
        string json = await _storage.LoadTextAsync("adventures", key);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("The configuration for the current game could not be found");
        }
        
        // Deserialize the JSON into a BasiliskKernelConfig
        BasiliskKernelConfig? kernelConfig = JsonConvert.DeserializeObject<BasiliskKernelConfig>(json);
        if (kernelConfig is null)
        {
            _logger.Warning("No configuration found for {Key}", key);
            throw new InvalidOperationException("The configuration for the current game could not be loaded");
        }
        
        // Set up Semantic Kernel
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(_config.AzureOpenAiDeploymentName,
            _config.AzureOpenAiEndpoint,
            _config.AzureOpenAiKey);

        builder.Services.AddLogging(s => s.AddSerilog(_logger, dispose: true));

        _kernel = builder.Build();

        // Set up services
        _chat = _kernel.GetRequiredService<IChatCompletionService>();

        // TODO: Support multiple agents eventually
        BasiliskAgentConfig agent = kernelConfig.Agents.First();
        
        // TODO: This should come from the individual game being played and possibly apply to multiple agents
        _history.AddSystemMessage(agent.SystemPrompt);

        // Add Plugins
        _kernel.RegisterBasiliskPlugins(_services);

        // If the config calls for it, make an initial request
        if (!string.IsNullOrWhiteSpace(kernelConfig.InitialPrompt))
        {
            return await ChatAsync(kernelConfig.InitialPrompt);
        }
        else
        {
            return new ChatResult
            {
                Message = "The game is ready to begin",
                Blocks = _context.Blocks
            };
        }
    }

    public async Task<ChatResult> ChatAsync(string message)
    {
        _logger.Information("{Agent}: {Message}", "User", message);
        _history.AddUserMessage(message); // TODO: We may need to move to a sliding window history approach
        _context.BeginNewRequest(message);

        if (_kernel == null || _chat == null)
        {
            throw new InvalidOperationException("The kernel has not been initialized");
        }

        string? response;
        try
        {
            ChatMessageContent result = await _chat.GetChatMessageContentAsync(_history, _executionSettings, _kernel);
            _history.Add(result);

            _logger.Information("{Agent}: {Message}", "User", message);

            response = result.Content;
        }
        catch (HttpOperationException ex)
        {
            _logger.Error(ex, "HTTP Error: {Message}", ex.Message);
            
            if (ex.InnerException is ClientResultException && ex.Message.Contains("content management", StringComparison.OrdinalIgnoreCase)) 
            {
                response = "I'm afraid that message is a bit too spicy for what I'm allowed to process. Can you try something else?";
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