using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class AdventureRunner
{
    private readonly ApiClient _client;
    private readonly RequestContextService _context;
    private readonly ILogger<AdventureRunner> _logger;

    public AdventureRunner(ApiClient client,
        ILogger<AdventureRunner> logger,
        RequestContextService context)
    {
        _client = client;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> RunAsync(AdventureInfo adventure)
    {
        _logger.LogDebug("Session Start");

        // Kick off the conversation
        ChatResult? result = null;
        await AnsiConsole.Status().StartAsync("Initializing the Game Master...",
            async _ => result = await _client.StartGameMasterConversationAsync(adventure.RowKey));
        result.Render();

        // This loop lets the user interact with the kernel until they end the session
        List<ChatMessage> history = result!.Replies.ToList();
        await RunMainLoopAsync(result.Id, history);

        _logger.LogDebug("Session End");

        return true;
    }

    private async Task RunMainLoopAsync(Guid conversationId, List<ChatMessage> history)
    {
        do
        {
            AnsiConsole.WriteLine();
            string prompt = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]Player[/]: "));

            if (prompt.IsExitCommand())
            {
                _context.CurrentAdventure = null;
            }
            else
            {
                _logger.LogInformation("> {Message}", prompt);

                await ChatWithKernelAsync(prompt, conversationId, history);
            }
        } while (_context.CurrentAdventure is not null);
    }

    private async Task ChatWithKernelAsync(string userMessage, Guid conversationId, List<ChatMessage> history)
    {
        ChatResult? response = null;
        await AnsiConsole.Status().StartAsync("The Game Master is thinking...",
            async _ =>
            {
                response = await _client.ChatWithGameMasterAsync(new ChatRequest
                {
                    Id = conversationId,
                    User = _client.Username,
                    Message = userMessage,
                    History = history
                }, _context.CurrentAdventure!.RowKey);
                
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