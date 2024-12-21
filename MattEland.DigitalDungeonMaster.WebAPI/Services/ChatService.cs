using System.Text;
using MattEland.DigitalDungeonMaster.Agents.GameMaster;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder;
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
    private readonly IFileStorageService _storage;
    private readonly AdventuresService _adventuresService;
    private readonly AgentConfigurationService _agentConfigService;
    private readonly RequestContextService _context;

    public ChatService(ILogger<ChatService> logger,
        AppUser user,
        IServiceProvider services,
        IFileStorageService storage,
        AdventuresService adventuresService,
        AgentConfigurationService agentConfigService,
        RequestContextService context)
    {
        _logger = logger;
        _user = user;
        _services = services;
        _storage = storage;
        _adventuresService = adventuresService;
        _agentConfigService = agentConfigService;
        _context = context;
    }

    public async Task<IChatResult> ChatAsync(AdventureInfo adventure, IChatRequest request)
    {
        // Store context
        _context.CurrentUser = _user.Name;
        _context.CurrentAdventure = adventure;
        
        // Log the request
        _logger.LogInformation("{User} to {Bot}: {Message}", _user.Name, request.RecipientName, request.Message);
        if (request.History is null || !request.History.Any())
        {
            _logger.LogWarning("No history was provided in the request");
        }
        
        // Initialize the agent
        AgentConfig config = _agentConfigService.GetAgentConfiguration(request.RecipientName ?? "Game Master");
        GameMasterAgent agent = _services.GetRequiredService<GameMasterAgent>();
        await LoadGameMasterPromptAsync(adventure, config);
        agent.Initialize(_services, config);

        // Chat
        return await SendChatAsync(agent, request);
    }

    private async Task LoadGameMasterPromptAsync(AdventureInfo adventure, AgentConfig config)
    {
        // Load any contextual prompt information
        StringBuilder promptBuilder = new();
        await AddStoryDetailsToPromptBuilderAsync(adventure, promptBuilder);
        if (adventure.Status == AdventureStatus.InProgress)
        {
            await AddRecapToPromptBuilderAsync(adventure, promptBuilder);
        }
        config.AdditionalPrompt = promptBuilder.ToString();
    }

    public async Task<IChatResult> StartChatAsync(AdventureInfo adventure)
    {
        // Store context
        _context.CurrentUser = _user.Name;
        _context.CurrentAdventure = adventure;

        // Assign an ID
        Guid chatId = Guid.NewGuid();
        _logger.LogInformation("Chat {Id} started with {User} in adventure {Adventure}", chatId, _user.Name,
            adventure.Name);
        
        // Initialize the agent
        AgentConfig config = _agentConfigService.GetAgentConfiguration("Game Master");
        GameMasterAgent agent = _services.GetRequiredService<GameMasterAgent>();
        await LoadGameMasterPromptAsync(adventure, config);
        agent.Initialize(_services, config);

        // Make the initial request
        ChatRequest<object> request = new()
        {
            User = _user.Name,
            RecipientName = agent.Name,
            Message = (adventure.Status == AdventureStatus.ReadyToLaunch) switch
            {
                true => config.NewCampaignPrompt ?? throw new InvalidOperationException("No new campaign prompt found"),
                false => config.ResumeCampaignPrompt ?? throw new InvalidOperationException("No resume campaign prompt found")
            }
        };

        return await SendChatAsync(agent, request);
    }

    private async Task<IChatResult> SendChatAsync(IChatAgent agent, IChatRequest request)
    {
        IChatResult result = await agent.ChatAsync(request, _user.Name);

        // Send the result back
        _logger.LogInformation("{Bot} to {User}: {Message}", agent.Name, _user.Name,
            result.Replies!.Count() != 1
                ? "Multiple replies"
                : result.Replies!.First().Message);

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
        NewGameSettingInfo? setting = await _adventuresService.LoadStorySettingsAsync(adventure);
        if (setting is not null)
        {
            promptBuilder.AppendLine("The adventure description is " + setting.GameSettingDescription);
            promptBuilder.AppendLine("The desired gameplay style is " + setting.DesiredGameplayStyle);
            promptBuilder.AppendLine("The main character is " + setting.PlayerCharacterName + ", a " +
                                     setting.PlayerCharacterClass + ". " + setting.PlayerDescription);
            promptBuilder.AppendLine("The campaign objective is " + setting.CampaignObjective);
            if (adventure.Status == AdventureStatus.ReadyToLaunch)
            {
                promptBuilder.AppendLine("The first session objective is " + setting.FirstSessionObjective);
            }
        }
        else
        {
            _logger.LogError("No settings found for adventure {Adventure}", adventure.RowKey);
            throw new InvalidOperationException($"No settings found for adventure {adventure.Name}");
        }
    }

    public async Task<ChatResult<NewGameSettingInfo>> StartWorldBuilderChatAsync(AdventureInfo adventure, NewGameSettingInfo setting)
    {
        // Store context
        _context.CurrentUser = _user.Name;
        _context.CurrentAdventure = adventure;

        // Assign an ID
        Guid chatId = Guid.NewGuid();
        _logger.LogInformation("World Builder Chat {Id} started with {User} in adventure {Adventure}", chatId, _user.Name,
            adventure.Name);
        
        // Initialize the agent
        AgentConfig config = _agentConfigService.GetAgentConfiguration("World Builder");
        WorldBuilderAgent agent = _services.GetRequiredService<WorldBuilderAgent>();
        agent.Initialize(_services, config);

        // Make the initial request
        ChatRequest<NewGameSettingInfo> request = new()
        {
            User = _user.Name,
            RecipientName = agent.Name,
            Data = setting,
            Message = "Greet the player and ask them to describe the world they want to play in and the character they want to play as."
        };

        return (ChatResult<NewGameSettingInfo>)await SendChatAsync(agent, request);
    }
    
    
    public async Task<ChatResult<NewGameSettingInfo>> ContinueWorldBuilderChatAsync(ChatRequest<NewGameSettingInfo> request, AdventureInfo adventure)
    {
        // Store context
        _context.CurrentUser = _user.Name;
        _context.CurrentAdventure = adventure;

        // Assign an ID
        _logger.LogInformation("Continuing World Builder Chat {Id} from {User} in adventure {Adventure}: {Message}", request.Id, _user.Name,
            adventure.Name, request.Message);
        
        // Initialize the agent
        AgentConfig config = _agentConfigService.GetAgentConfiguration("World Builder");
        _logger.LogTrace("Using prompt {Prompt}", config.FullPrompt);
        
        WorldBuilderAgent agent = _services.GetRequiredService<WorldBuilderAgent>();
        agent.Initialize(_services, config);

        return (ChatResult<NewGameSettingInfo>)await SendChatAsync(agent, request);
    }
}