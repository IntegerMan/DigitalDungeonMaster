using MattEland.DigitalDungeonMaster.Agents.GameMaster;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class AdventureRunner
{
    private readonly GameMasterAgent _gm;
    private readonly IServiceProvider _serviceProvider;
    private readonly RequestContextService _context;
    private readonly ILogger<AdventureRunner> _logger;
    private readonly StorageDataService _storageService;

    public AdventureRunner(GameMasterAgent gm, 
        IServiceProvider serviceProvider, 
        ILogger<AdventureRunner> logger, 
        StorageDataService storageService,
        RequestContextService context)
    {
        _gm = gm;
        _serviceProvider = serviceProvider;
        _context = context;
        _logger = logger;
        _storageService = storageService;
    }
    
    public async Task<bool> RunAsync(AdventureInfo adventure, bool isNewAdventure)
    {
        _logger.LogDebug("Session Start");
        
        
        await AnsiConsole.Status().StartAsync("Loading Adventure Settings...",
            async _ =>
            {
                string settingsPath = $"{adventure.Container}/StorySetting.json";
                string? json = await _storageService.LoadTextOrDefaultAsync("adventures", settingsPath);

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
                        if (isNewAdventure)
                        {
                            additionalPrompt.AppendLine("The first session objective is " + setting.FirstSessionObjective);
                        }
                        
                        _gm.AdditionalPrompt = additionalPrompt.ToString();
                        
                        _logger.LogDebug("Adding additional prompt to GM: {Prompt}", _gm.AdditionalPrompt);
                    }
                }
                else
                {
                    _logger.LogWarning("No settings found for adventure {Adventure} at {SettingsPath}", adventure, settingsPath);
                }
            });
        
        await AnsiConsole.Status().StartAsync("Initializing the Game Master...",
            async _ =>
            {
                _gm.IsNewAdventure = isNewAdventure;
                ChatResult result = await _gm.InitializeAsync(_serviceProvider);
                result.Blocks.Render();
            });

        // This loop lets the user interact with the kernel until they end the session
        await RunMainLoopAsync();

        _logger.LogDebug("Session End");
        
        return true;
    }
    
    private async Task RunMainLoopAsync()
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
                
                await ChatWithKernelAsync(prompt);
            }
        } while (_context.CurrentAdventure is not null);
    }
    
    private async Task ChatWithKernelAsync(string userMessage)
    {
        ChatResult? response = null;
        await AnsiConsole.Status().StartAsync("The Game Master is thinking...",
            async _ => { response = await _gm.ChatAsync(new ChatRequest
            {
                Message = userMessage
            }); 
        });

        _logger.LogInformation("{Message}", response!.Message);
        response.Blocks.Render();
    }
}