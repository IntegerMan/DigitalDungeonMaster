using System.Text;
using MattEland.DigitalDungeonMaster.Agents.GameMaster;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.WebAPI.Services;

public class ChatService
{
    private readonly ILogger<ChatService> _logger;
    private readonly AppUser _user;
    private readonly IServiceProvider _services;
    private readonly IStorageService _storage;
    private readonly RequestContextService _context;

    public ChatService(ILogger<ChatService> logger, AppUser user, IServiceProvider services, IStorageService storage, RequestContextService context)
    {
        _logger = logger;
        _user = user;
        _services = services;
        _storage = storage;
        _context = context;
    }
    
    public async Task<ChatResult> ChatAsync(ChatRequest request)
    {
        _context.CurrentUser = _user.Name;
        // TODO: Set adventure context
        
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

    public async Task<ChatResult> StartChatAsync(AdventureInfo adventure)
    {
        // Store context
        _context.CurrentUser = _user.Name;
        _context.CurrentAdventure = adventure;
        
        // Assign an ID
        Guid chatId = Guid.NewGuid();
        _logger.LogInformation("Chat {Id} started with {User} in adventure {Adventure}", chatId, _user.Name, adventure.Name);
        
        // Get our game master
        _logger.LogDebug("Building Game Master agent");
        GameMasterAgent gm = _services.GetRequiredService<GameMasterAgent>();
        gm.IsNewAdventure = adventure.Status == AdventureStatus.New;
        
        // Fetch information on the adventure and add it to the prompt
        string settingsPath = $"{adventure.Container}/StorySetting.json";
        string? json = await _storage.LoadTextOrDefaultAsync("adventures", settingsPath);
        if (!string.IsNullOrWhiteSpace(json))
        {
            _logger.LogDebug("Settings found for adventure {Adventure} at {SettingsPath}", adventure, settingsPath);
                    
            NewGameSettingInfo? setting = JsonConvert.DeserializeObject<NewGameSettingInfo>(json);
            if (setting is not null)
            {
                StringBuilder additionalPrompt = new();
                additionalPrompt.AppendLine("The adventure description is " + setting.GameSettingDescription);
                additionalPrompt.AppendLine("The desired gameplay style is " + setting.DesiredGameplayStyle);
                additionalPrompt.AppendLine("The main character is " + setting.PlayerCharacterName + ", a " + setting.PlayerCharacterClass + ". " + setting.PlayerDescription);
                additionalPrompt.AppendLine("The campaign objective is " + setting.CampaignObjective);
                if (adventure.Status == AdventureStatus.New)
                {
                    additionalPrompt.AppendLine("The first session objective is " + setting.FirstSessionObjective);
                } 
                else
                {
                    // TODO: Would be a good place to inject the recap
                }
                        
                gm.AdditionalPrompt = additionalPrompt.ToString();
                _logger.LogDebug("Adding additional prompt to GM: {Prompt}", gm.AdditionalPrompt);
            }
        }
        else
        {
            _logger.LogWarning("No settings found for adventure {Adventure} at {SettingsPath}", adventure, settingsPath);
        }
        
        // Start the chat
        ChatResult result = await gm.InitializeAsync(_services, _user.Name);
        
        // Send the result back
        _logger.LogInformation("{Bot} is replying to {User}: {Message}", gm.Name, _user.Name, result.Replies.Count() != 1 
            ? "Multiple replies" 
            : result.Replies.First().Message);

        return result;
    }
}