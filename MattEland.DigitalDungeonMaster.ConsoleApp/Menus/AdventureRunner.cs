using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        
        /*
        await AnsiConsole.Status().StartAsync("Loading Adventure Settings...",
            async _ =>
            {
                string settingsPath = $"{adventure.Container}/StorySetting.json";
                string? json = null; // TODO: await _storageService.LoadTextOrDefaultAsync("adventures", settingsPath);

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
                        
                        // TODO: _gm.AdditionalPrompt = additionalPrompt.ToString();
                        //_logger.LogDebug("Adding additional prompt to GM: {Prompt}", _gm.AdditionalPrompt);
                    }
                }
                else
                {
                    _logger.LogWarning("No settings found for adventure {Adventure} at {SettingsPath}", adventure, settingsPath);
                }
            });
            */
        
        await AnsiConsole.Status().StartAsync("Initializing the Game Master...",
            async _ =>
            {
                // TODO: _gm.IsNewAdventure = isNewAdventure;
                ChatResult result = await _client.StartGameMasterConversationAsync(adventure.Owner, adventure.RowKey);
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
            async _ => { response = await _client.ChatWithGameMasterAsync(new ChatRequest
            {
                Message = userMessage
            }); 
        });

        _logger.LogInformation("{Message}", response!.Message);
        response.Blocks.Render();
    }
}