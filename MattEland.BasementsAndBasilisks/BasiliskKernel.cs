namespace MattEland.BasementsAndBasilisks;

using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

public class BasiliskKernel
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly ChatHistory _history;

    public BasiliskKernel(string openAiDeploymentName, string openAiEndpoint, string openAiApiKey)
    {
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(openAiDeploymentName, 
            openAiEndpoint, 
            openAiApiKey);
        
        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

        _kernel = builder.Build();
        
        // Set up services
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
        _history = new ChatHistory();
        _history.AddSystemMessage("You are a dungeon master directing play of a game similar to the 5th edition of Dungeons and Dragons. The user represents the only player in the game.");
    }

    public async Task<string> ChatAsync(string message)
    {
        _history.AddUserMessage(message);
        ChatMessageContent result = await _chat.GetChatMessageContentAsync(_history, kernel: _kernel);

        string response = result.Content ?? "I'm afraid I can't respond to that right now.";
        _history.AddAssistantMessage(response);
        
        return response;
    }
}