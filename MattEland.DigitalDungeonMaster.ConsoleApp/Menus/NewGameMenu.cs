using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.GameManagement.Services;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class NewGameMenu
{
    private readonly RequestContextService _context;
    private readonly RulesetService _rulesetService;
    private readonly AdventuresService _adventuresService;
    private readonly WorldBuilderAgent _worldBuilder;
    private readonly IServiceProvider _services;

    public NewGameMenu(RequestContextService context, RulesetService rulesetService, AdventuresService adventuresService, WorldBuilderAgent worldBuilder, IServiceProvider services)
    {
        _context = context;
        _rulesetService = rulesetService;
        _adventuresService = adventuresService;
        _worldBuilder = worldBuilder;
        _services = services;
    }

    public async Task<bool> RunAsync()
    {
        List<Ruleset> rulesets = [];
        await AnsiConsole.Status().StartAsync("Loading rulesets...",
            async _ => rulesets.AddRange(await _rulesetService.LoadRulesetsAsync(_context.CurrentUser!)));

        if (!rulesets.Any())
        {
            AnsiConsole.MarkupLine("[Red]No rulesets found. Please create a ruleset first.[/]");
            return true;
        }

        // Select a ruleset
        rulesets.Add(new Ruleset { Name = "Cancel", Key = "Cancel", Owner = _context.CurrentUser!});
        Ruleset ruleset = AnsiConsole.Prompt(new SelectionPrompt<Ruleset>()
            .Title("Select the ruleset for your new adventure:")
            .AddChoices(rulesets)
            .UseConverter(r => r.Name));
        
        // TODO: We may want to get a creation prompt from the ruleset
        
        if (ruleset.Key != "Cancel")
        {
            ChatResult? response = null;
            await AnsiConsole.Status().StartAsync("Initializing the world builder...",
                async _ => response = await _worldBuilder.InitializeAsync(_services));
            
            response!.Blocks.Render();
            bool operationCancelled = false;
            
            // Main input loop
            do
            {
                string message = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]Player[/]:"));

                if (message.IsExitCommand())
                {
                    operationCancelled = true;
                }
                else
                {
                    await AnsiConsole.Status().StartAsync("Waiting for world builder...",
                        async _ => response = await _worldBuilder.ChatAsync(new ChatRequest
                        {
                            Message = message
                        }));

                    response?.Blocks.Render();
                }
            } while (!operationCancelled && !_worldBuilder.HasCreatedWorld);

            NewGameSettingInfo? setting = _worldBuilder.SettingInfo;
            
            // Create the adventure
            if (!operationCancelled && setting is not null)
            {
                string key = setting.CampaignName.Replace(" ", string.Empty); // TODO: Check for restricted characters on blob names
                AdventureInfo adventure = new()
                {
                    Name = setting.CampaignName,
                    Ruleset = ruleset.Key,
                    Description = setting.GameSettingDescription,
                    Owner = _context.CurrentUser!,
                    Container = $"{_context.CurrentUser!}_{key}",
                    RowKey = key
                };

                await AnsiConsole.Status().StartAsync("Creating adventure...",
                    async _ => await _adventuresService.CreateAdventureAsync(adventure));
            }
        }

        return true;
    }
}