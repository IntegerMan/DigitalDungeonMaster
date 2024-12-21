using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class AdventureRunner
{
    private readonly ApiClient _client;
    private readonly ILogger<AdventureRunner> _logger;

    public AdventureRunner(
        ApiClient client,
        ILogger<AdventureRunner> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> RunAsync(AdventureInfo adventure)
    {
        _logger.LogDebug("Session Start");

        // Kick off the conversation
        IChatResult? result = null;
        await AnsiConsole.Status().StartAsync("Initializing the Game Master...",
            async _ => result = await _client.StartGameMasterConversationAsync(adventure.RowKey));
        result.Render();

        // This loop lets the user interact with the kernel until they end the session
        List<ChatMessage> history = result!.Replies.ToList();
        await RunMainLoopAsync(result.Id, history, adventure);

        _logger.LogDebug("Session End");

        return true;
    }

    private async Task RunMainLoopAsync(Guid conversationId, List<ChatMessage> history, AdventureInfo adventure)
    {
        do
        {
            AnsiConsole.WriteLine();
            string prompt = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]Player[/]: "));

            if (prompt.IsExitCommand())
            {
                break;
            }

            _logger.LogInformation("> {Message}", prompt);
            await ChatWithKernelAsync(prompt, conversationId, history, adventure);
        } while (true);
    }

    private async Task ChatWithKernelAsync(string userMessage, Guid conversationId, List<ChatMessage> history, AdventureInfo adventure)
    {
        IChatResult? response = null;
        await AnsiConsole.Status().StartAsync("The Game Master is thinking...",
            async _ =>
            {
                response = await _client.ChatWithGameMasterAsync(new ChatRequest<object>
                {
                    Id = conversationId,
                    User = _client.Username,
                    Message = userMessage,
                    History = history
                }, adventure.RowKey);
                
                // Update the history with our message and the bot's reply
                history.Add(new ChatMessage
                {
                    Author = _client.Username,
                    Message = userMessage
                });
                history.AddRange(response!.Replies);
            });

        response.Render();
    }
}