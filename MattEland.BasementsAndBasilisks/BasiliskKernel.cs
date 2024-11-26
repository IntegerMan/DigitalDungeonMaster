using MattEland.BasementsAndBasilisks.Plugins;

namespace MattEland.BasementsAndBasilisks;

using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class BasiliskKernel
{
    private readonly Kernel _kernel;
    private readonly OpenAIPromptExecutionSettings _executionSettings;
    private readonly IChatCompletionService _chat;
    private readonly ChatHistory _history;

    public BasiliskKernel(IServiceProvider services, string openAiDeploymentName, string openAiEndpoint,
        string openAiApiKey)
    {
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(openAiDeploymentName, 
            openAiEndpoint, 
            openAiApiKey);
        
        builder.Services.AddLogging(s => s.AddConsole().SetMinimumLevel(LogLevel.Trace));

        _kernel = builder.Build();
        
        // Set execution settings
        _executionSettings = new OpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // Set up services
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
        _history = new ChatHistory();
        _history.AddSystemMessage("You are a dungeon master directing play of a game called Basements and Basilisks. The user represents the only player in the game. Let the player make their own decisions, ask for skill checks and saving rolls when needed, and call functions to get your responses as needed.");
    
        // Add Plugins
        _kernel.Plugins.AddFromObject(services.GetRequiredService<RacesPlugin>(), "Races", services);
        _kernel.Plugins.AddFromObject(services.GetRequiredService<ClassesPlugin>(), "Classes", services);
        _kernel.Plugins.AddFromObject(services.GetRequiredService<QuestionAnsweringPlugin>(), "Oracle", services);
    }

    public async Task<string> ChatAsync(string message)
    {
        _history.AddUserMessage(message);
        ChatMessageContent result = await _chat.GetChatMessageContentAsync(_history, kernel: _kernel, executionSettings: _executionSettings);

        string response = result.Content ?? "I'm afraid I can't respond to that right now.";
        _history.AddAssistantMessage(response);
        
        return response;
    }
}