using System.ClientModel;
using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using OpenAI.Chat;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Core;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;
using DiagnosticBlock = MattEland.BasementsAndBasilisks.Blocks.DiagnosticBlock;

#pragma warning disable SKEXP0001

#pragma warning disable SKEXP0110

namespace MattEland.BasementsAndBasilisks;

public sealed class BasiliskKernel : IDisposable
{
    private Kernel? _kernel;
    private IChatCompletionService? _chat;
    private List<ChatHistory> _agentHistories;
    private bool _disposedValue;

    private readonly Logger _logger;
    private readonly RequestContextService _context;
    private readonly StorageDataService _storage;
    private readonly IServiceProvider _services;
    private readonly BasiliskConfig _config;
    private List<ChatCompletionAgent> _agents;
    private AgentGroupChat _groupChat;

    public BasiliskKernel(IServiceProvider services,
        BasiliskConfig config,
        string logPath)
    {
        // Get necessary services
        _services = services;
        _config = config;
        _context = services.GetRequiredService<RequestContextService>();
        _storage = services.GetRequiredService<StorageDataService>();

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

        // Diagnostics on agents being loaded (and their sequence)
        List<BasiliskAgentConfig> agentConfigs = kernelConfig.Agents.ToList();
        _context.AddBlock(new DiagnosticBlock
        {
            Header = $"Loading {agentConfigs.Count} Agent(s)",
            Metadata = string.Join(", ", agentConfigs.Select(a => a.Name))
        });

        // Set up Semantic Kernel
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(_config.AzureOpenAiDeploymentName,
            _config.AzureOpenAiEndpoint,
            _config.AzureOpenAiKey);

        builder.Services.AddLogging(s => s.AddSerilog(_logger, dispose: true));

        _kernel = builder.Build();

        // Set up services
        _chat = _kernel.GetRequiredService<IChatCompletionService>();

        // Load the agents
        _agents = new List<ChatCompletionAgent>(agentConfigs.Count);
        _agentHistories = new List<ChatHistory>(agentConfigs.Count);
        for (int i = 0; i < agentConfigs.Count; i++)
        {
            BasiliskAgentConfig agentConfig = agentConfigs[i];
            ChatCompletionAgent agent = new()
            {
                Name = agentConfig.Name,
                Instructions = agentConfig.SystemPrompt,
                Kernel = _kernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                }),
                HistoryReducer = null, // TODO: This would be good to use!
            };
            _agents.Add(agent);
            _agentHistories.Add(new ChatHistory(agentConfig.SystemPrompt));
        }

        _groupChat = new AgentGroupChat(_agents.ToArray())
        {
            /*
            ExecutionSettings = new AgentGroupChatSettings
            {
                SelectionStrategy = new SequentialSelectionStrategy
                {
                    InitialAgent = _agents.First()
                },
                TerminationStrategy =
                {
                    MaximumIterations = agents.Count,
                    Agents = [agents.Last()],
                    AutomaticReset = true
                }
            }
            */
        };

        // Add Plugins
        _kernel.RegisterBasiliskPlugins(_services);

        // If the config calls for it, make an initial request
        if (!string.IsNullOrWhiteSpace(kernelConfig.InitialPrompt))
        {
            return await ChatAsync(kernelConfig.InitialPrompt, clearHistory: false);
        }

        // If we got here, the game did not provide an initial prompt
        return new ChatResult
        {
            Message = "The game is ready to begin",
            Blocks = _context.Blocks
        };
    }

    public async Task<ChatResult> ChatAsync(string message, bool clearHistory = true)
    {
        _logger.Information("{Agent}: {Message}", "User", message);

        if (_kernel == null || _chat == null)
        {
            throw new InvalidOperationException("The kernel has not been initialized");
        }

        foreach (var history in _agentHistories)
        {
            history.AddUserMessage(message);
        }
        _context.BeginNewRequest(message, clearHistory);

        string? response;
        try
        {
            _logger.Information("{Agent}: {Message}", "User", message);

            response = "There was no response from the game master agent";
            
            _groupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, message));
            foreach (var agent in _agents)
            {
                ChatMessageContent agentResponse = await _groupChat.InvokeAsync(agent).FirstAsync();
                response = agentResponse.Content;
            }
            /*
            IAsyncEnumerable<ChatMessageContent> results = _agentGroupChat.InvokeAsync();
            await foreach (var result in results)
            {
                _context.AddBlock(new DiagnosticBlock
                {
                    Header = result.AuthorName ?? "Response",
                    Metadata = result.Content
                });
                _logger.Information("{Agent}: {Message}", result.AuthorName, result.Content);
                response = result.Content;
            }
            */
        }
        catch (HttpOperationException ex)
        {
            _logger.Error(ex, "HTTP Error: {Message}", ex.Message);

            if (ex.InnerException is ClientResultException &&
                ex.Message.Contains("content management", StringComparison.OrdinalIgnoreCase))
            {
                response =
                    "I'm afraid that message is a bit too spicy for what I'm allowed to process. Can you try something else?";
            }
            else
            {
#if DEBUG
                response = ex.Message;
#else
                response =
 "I couldn't handle your request due to an error. Please try again or report this issue if it persists.";
#endif
            }
        }

        response ??= "I can't respond right now.";

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