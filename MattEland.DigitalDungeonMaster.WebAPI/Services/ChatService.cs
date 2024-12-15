using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;

namespace MattEland.DigitalDungeonMaster.WebAPI.Services;

public class ChatService
{
    private readonly ILogger<ChatService> _logger;
    private readonly AppUser _user;

    public ChatService(ILogger<ChatService> logger, AppUser user)
    {
        _logger = logger;
        _user = user;
    }
    
    public async Task<ChatResult> ChatAsync(ChatRequest request)
    {
        Guid chatId;
        if (!request.Id.HasValue)
        {
            chatId = Guid.NewGuid();
            _logger.LogWarning("Incoming message had no chat ID. Assigning new chat ID {Id}", chatId);
        }
        else
        {
            chatId = request.Id.Value;
        }

        request.RecipientName ??= "Test Bot";
        _logger.LogInformation("Received message in chat {Id} to {Bot} from {User}: {Message}", chatId, request.RecipientName, _user.Name, request.Message);
                
        string reply = "Blood for the Blood God!";
        _logger.LogInformation("{Bot} is replying to {User}: {Message}", request.RecipientName, _user.Name, reply);


        ChatResult result = new ChatResult
        {
            Id = chatId,
            Replies =
            [
                new ChatMessage
                {
                    Author = request.RecipientName,
                    Message = reply,
                }
            ]
        };

        await Task.CompletedTask;
        
        return result;
    }

    public async Task<ChatResult> StartChatAsync(string username, AdventureInfo adventure)
    {
        Guid chatId = Guid.NewGuid();
        _logger.LogInformation("Chat {Id} started with {User} in adventure {Adventure}", chatId, username, adventure.Name);
                
        string reply = "WAAAARG!";
        string agent = "Game Master";
        _logger.LogInformation("{Bot} is replying to {User}: {Message}", agent, _user.Name, reply);
                
        ChatResult result = new ChatResult
        {
            Id = chatId,
            Replies =
            [
                new ChatMessage
                {
                    Author = agent,
                    Message = reply,
                }
            ]
        };

        await Task.CompletedTask;
        
        return result;
    }
}