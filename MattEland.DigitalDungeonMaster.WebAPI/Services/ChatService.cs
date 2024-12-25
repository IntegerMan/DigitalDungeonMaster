using System.Diagnostics;
using System.Text;
using MattEland.DigitalDungeonMaster.Agents.GameMaster;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;
using MattEland.DigitalDungeonMaster.WebAPI.Models;

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
    private readonly ActivitySource _activitySource;

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
        _activitySource = new ActivitySource(GetType().Assembly.FullName ??
                                             GetType().Assembly.GetName().Name ?? GetType().FullName ?? "Unknown");
    }

    public async Task<IChatResult> ChatAsync(AdventureInfo adventure, GameChatRequest request)
    {
        using Activity? activity = _activitySource.StartActivity($"User: {request.Message.Message}");
        activity?.AddBaggage("User", _user.Name);
        activity?.AddBaggage("Adventure", adventure.RowKey);
        activity?.AddBaggage("Conversation", request.Id.ToString());
        
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
        IChatAgent<GameChatRequest, GameChatResult> agent = LoadChatAgent(config);
        
        await LoadGameMasterPromptAsync(adventure, config);
        agent.Initialize(_services, config);

        // Chat
        GameChatResult result = await SendChatAsync(agent, request);
        result.AvailableAgents = _agentConfigService.GetAvailableAgents();
        
        return result;
    }

    private IChatAgent<GameChatRequest, GameChatResult> LoadChatAgent(AgentConfig config)
    {
        Type? agentType = Type.GetType(config.AgentType);
        if (agentType is null)
        {
            throw new InvalidOperationException($"Agent type {config.AgentType} not found");
        }
        
        IChatAgent<GameChatRequest, GameChatResult> agent = (IChatAgent<GameChatRequest, GameChatResult>)_services.GetRequiredService(agentType);
        return agent;
    }

    private async Task LoadGameMasterPromptAsync(AdventureInfo adventure, AgentConfig config)
    {
        // Load any contextual prompt information
        // TODO: This will likely need to change per agent
        StringBuilder promptBuilder = new();
        await AddStoryDetailsToPromptBuilderAsync(adventure, promptBuilder);
        if (adventure.Status == AdventureStatus.InProgress)
        {
            await AddRecapToPromptBuilderAsync(adventure, promptBuilder);
        }
        config.AdditionalPrompt = promptBuilder.ToString();
    }

    public async Task<GameChatResult> StartChatAsync(AdventureInfo adventure)
    {
        using Activity? activity = _activitySource.StartActivity($"Starting chat session for adventure {adventure.RowKey}");
        activity?.AddBaggage("User", _user.Name);
        activity?.AddBaggage("Adventure", adventure.RowKey);
        
        // Store context
        _context.CurrentUser = _user.Name;
        _context.CurrentAdventure = adventure;

        // Assign an ID
        Guid chatId = Guid.NewGuid();
        activity?.AddBaggage("Conversation", chatId.ToString());
        _logger.LogInformation("Chat {Id} started with {User} in adventure {Adventure}", chatId, _user.Name,
            adventure.Name);
        
        // Initialize the agent
        AgentConfig config = _agentConfigService.GetAgentConfiguration("Game Master");
        IChatAgent<GameChatRequest, GameChatResult> agent = LoadChatAgent(config);
        
        await LoadGameMasterPromptAsync(adventure, config);
        agent.Initialize(_services, config);

        // Make the initial request
        GameChatRequest request = new()
        {
            User = _user.Name,
            RecipientName = agent.Name,
            Message = new ChatMessage
            {
                Author = _user.Name,
                Message = (adventure.Status == AdventureStatus.ReadyToLaunch) switch
                {
                    true => config.NewCampaignPrompt ?? throw new InvalidOperationException("No new campaign prompt found"),
                    false => config.ResumeCampaignPrompt ?? throw new InvalidOperationException("No resume campaign prompt found")
                }
            }
        };

        GameChatResult result = await SendChatAsync(agent, request);
        result.AvailableAgents = _agentConfigService.GetAvailableAgents();
        
        return result;
    }

    private async Task<GameChatResult> SendChatAsync(IChatAgent<GameChatRequest, GameChatResult> agent, GameChatRequest request)
    {
        GameChatResult result = await agent.ChatAsync(request, _user.Name);

        // Send the result back
        _logger.LogInformation("{Bot} to {User}: {Message}", agent.Name, _user.Name,
            result.Replies.Count() != 1
                ? "Multiple replies"
                : result.Replies.First().Message);

        return result;
    }
    
    private async Task<WorldBuilderChatResult> SendChatAsync(WorldBuilderAgent agent, WorldBuilderChatRequest request)
    {
        WorldBuilderChatResult result = await agent.ChatAsync(request, _user.Name);

        // Send the result back
        _logger.LogInformation("{Bot} to {User}: {Message}", agent.Name, _user.Name,
            result.Replies.Count() != 1
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
            _logger.LogWarning("No settings found for adventure {Adventure}. Play experience may be diminished.", adventure.RowKey);
        }
    }

    public async Task<WorldBuilderChatResult> StartWorldBuilderChatAsync(AdventureInfo adventure, NewGameSettingInfo setting)
    {
        using Activity? activity = _activitySource.StartActivity($"Starting worldbuilder chat session for adventure {adventure.RowKey}");
        activity?.AddBaggage("User", _user.Name);
        activity?.AddBaggage("Adventure", adventure.RowKey);
        
        // Store context
        _context.CurrentUser = _user.Name;
        _context.CurrentAdventure = adventure;

        // Assign an ID
        Guid chatId = Guid.NewGuid();
        activity?.AddBaggage("Conversation", chatId.ToString());
        _logger.LogInformation("World Builder Chat {Id} started with {User} in adventure {Adventure}", chatId, _user.Name,
            adventure.Name);
        
        // Initialize the agent
        AgentConfig config = _agentConfigService.GetAgentConfiguration("World Builder");
        WorldBuilderAgent agent = _services.GetRequiredService<WorldBuilderAgent>();
        agent.Initialize(_services, config);

        // Make the initial request
        WorldBuilderChatRequest request = new()
        {
            User = _user.Name,
            RecipientName = agent.Name,
            Data = setting,
            Message = new ChatMessage
            {
                Author = _user.Name,
                Message = "Greet the player and ask them to describe the world they want to play in and the character they want to play as."
            }
        };

        return await SendChatAsync(agent, request);
    }
    
    
    public async Task<WorldBuilderChatResult> ContinueWorldBuilderChatAsync(WorldBuilderChatRequest request, AdventureInfo adventure)
    {
        using Activity? activity = _activitySource.StartActivity($"User: {request.Message.Message}");
        activity?.AddBaggage("User", _user.Name);
        activity?.AddBaggage("Adventure", adventure.RowKey);
        activity?.AddBaggage("Conversation", request.Id.ToString());
        
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

        return await SendChatAsync(agent, request);
    }
}