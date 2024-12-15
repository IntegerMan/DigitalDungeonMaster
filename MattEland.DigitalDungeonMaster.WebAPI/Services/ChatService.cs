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
        request.RecipientName ??= "Test Bot";
        _logger.LogInformation("Received message to {Bot} from {User}: {Message}", request.RecipientName, _user.Name, request.Message);
                
        string reply = "Blood for the Blood God!";
        _logger.LogInformation("{Bot} is replying to {User}: {Message}", request.RecipientName, _user.Name, reply);
                
        ChatResult result = new ChatResult
        {
            Replies =
            [
                new ChatMessage
                {
                    Author = request.RecipientName,
                    Message = reply,
                }
            ]
        };
        
        return result;
    }
}