using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Core;

namespace MattEland.BasementsAndBasilisks;

public class BasiliskKernel : IDisposable
{
    private readonly Kernel _kernel;
    private readonly OpenAIPromptExecutionSettings _executionSettings;
    private readonly IChatCompletionService _chat;
    private readonly ChatHistory _history;
    private bool disposedValue;

    private readonly Logger _logger;

    public BasiliskKernel(IServiceProvider services, string openAiDeploymentName, string openAiEndpoint,
        string openAiApiKey)
    {
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(openAiDeploymentName, 
            openAiEndpoint, 
            openAiApiKey);

        string logPath = Path.Combine(Environment.CurrentDirectory, "Basilisk.log");
        Console.WriteLine($"Logging to {logPath}");

        _logger = new LoggerConfiguration()
           .MinimumLevel.Verbose()
           .WriteTo.File(new CompactJsonFormatter(), path: logPath)
           .CreateLogger();

        builder.Services.AddLogging(s => s.AddSerilog(_logger, dispose: true));

        _kernel = builder.Build();
        
        // Set execution settings
        _executionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // Set up services
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
        _history = new ChatHistory();
        _history.AddSystemMessage("""
        You are a dungeon master directing play of a game called Basements and Basilisks. 
        The user represents the only player in the game. Let the player make their own decisions, 
        ask for skill checks and saving rolls when needed, and call functions to get your responses as needed.
        Feel free to use markdown in your responses, but avoid lists.
        Ask the player what they'd like to do, but avoid railroading them or nudging them too much.
        """);
    
        // Add Plugins
        _kernel.RegisterBasiliskPlugins(services);
    }

    public async Task<string> ChatAsync(string message)
    {
        _logger.Information("{Agent}: {Message}", "User", message);

        _history.AddUserMessage(message);
        ChatMessageContent result = await _chat.GetChatMessageContentAsync(_history, kernel: _kernel, executionSettings: _executionSettings);

        string response = result.Content ?? "I'm afraid I can't respond to that right now.";
        _history.AddAssistantMessage(response);
        _logger.Information("{Agent}: {Message}", "User", message);

        
        return response;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _logger.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}