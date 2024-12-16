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
    private readonly AgentConfigurationService _agentConfigService;
    private readonly RequestContextService _context;

    public ChatService(ILogger<ChatService> logger, 
        AppUser user, 
        IServiceProvider services, 
        IStorageService storage, 
        AgentConfigurationService agentConfigService,
        RequestContextService context)
    {
        _logger = logger;
        _user = user;
        _services = services;
        _storage = storage;
        _agentConfigService = agentConfigService;
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
        
        // Load any contextual prompt information
        StringBuilder promptBuilder = new();
        await AddStoryDetailsToPromptBuilderAsync(adventure, promptBuilder);
        if (adventure.Status == AdventureStatus.InProgress)
        {
            await AddRecapToPromptBuilderAsync(adventure, promptBuilder);
        }
        string additionalPrompt = promptBuilder.ToString();
        _logger.LogDebug("Adding additional prompt to GM: {Prompt}", additionalPrompt);
        
        // Configure the agent
        AgentConfig config = _agentConfigService.GetAgentConfiguration(gm.Name);
        config.AdditionalPrompt = additionalPrompt;
        gm.Initialize(_services, config);

        // Make the initial request
        ChatRequest request = new ChatRequest
        {
            RecipientName = gm.Name,
            Message = (adventure.Status == AdventureStatus.New) switch
            {
                true => config.NewCampaignPrompt ?? throw new InvalidOperationException("No new campaign prompt found"),
                false => config.ResumeCampaignPrompt ?? throw new InvalidOperationException("No resume campaign prompt found")
            }
        };
        
        ChatResult result = await gm.ChatAsync(request, _user.Name);
        
        // Send the result back
        _logger.LogInformation("{Bot} is replying to {User}: {Message}", gm.Name, _user.Name, result.Replies.Count() != 1 
            ? "Multiple replies" 
            : result.Replies.First().Message);

        return result;
    }

    private async Task AddRecapToPromptBuilderAsync(AdventureInfo adventure, StringBuilder promptBuilder)
    {
        string? recap = await _storage.LoadTextOrDefaultAsync("adventures", $"{adventure.Container}/Recap.md");
        if (string.IsNullOrWhiteSpace(recap))
        {
            _logger.LogWarning("No recap was found for the last session");
        }
        else
        {
            _logger.LogDebug("Session recap loaded: {Recap}", recap);
                        
            promptBuilder.AppendLine("Here's a recap of the last session:");
            promptBuilder.AppendLine(recap);
        }
    }

    private async Task AddStoryDetailsToPromptBuilderAsync(AdventureInfo adventure, StringBuilder promptBuilder)
    {
        string settingsPath = $"{adventure.Container}/StorySetting.json";
        string? json = await _storage.LoadTextOrDefaultAsync("adventures", settingsPath);
        if (!string.IsNullOrWhiteSpace(json))
        {
            _logger.LogDebug("Settings found for adventure {Adventure} at {SettingsPath}", adventure, settingsPath);
            
            NewGameSettingInfo? setting = JsonConvert.DeserializeObject<NewGameSettingInfo>(json);
            if (setting is not null)
            {
                promptBuilder.AppendLine("The adventure description is " + setting.GameSettingDescription);
                promptBuilder.AppendLine("The desired gameplay style is " + setting.DesiredGameplayStyle);
                promptBuilder.AppendLine("The main character is " + setting.PlayerCharacterName + ", a " + setting.PlayerCharacterClass + ". " + setting.PlayerDescription);
                promptBuilder.AppendLine("The campaign objective is " + setting.CampaignObjective);
                if (adventure.Status == AdventureStatus.New)
                {
                    promptBuilder.AppendLine("The first session objective is " + setting.FirstSessionObjective);
                }
            }
        }
        else
        {
            _logger.LogWarning("No settings found for adventure {Adventure} at {SettingsPath}", adventure, settingsPath);
        }
    }
}