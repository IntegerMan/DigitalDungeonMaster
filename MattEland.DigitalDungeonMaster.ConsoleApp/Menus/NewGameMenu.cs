using MattEland.DigitalDungeonMaster.Agents.WorldBuilder;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;
using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.GameManagement.Services;
using MattEland.DigitalDungeonMaster.Services;
using Newtonsoft.Json;
using Spectre.Console.Json;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class NewGameMenu
{
    private readonly RequestContextService _context;
    private readonly RulesetService _rulesetService;
    private readonly AdventuresService _adventuresService;
    private readonly WorldBuilderAgent _worldBuilder;
    private readonly IServiceProvider _services;

    public NewGameMenu(RequestContextService context, RulesetService rulesetService,
        AdventuresService adventuresService, WorldBuilderAgent worldBuilder, IServiceProvider services)
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
        rulesets.Add(new Ruleset { Name = "Cancel", Key = "Cancel", Owner = _context.CurrentUser! });
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
                AnsiConsole.WriteLine();
                string message = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]Player[/] ([Cyan]/help[/] for command list):"));

                if (message.IsExitCommand())
                {
                    operationCancelled = true;
                }
                else if (message.StartsWith('/'))
                {
                    switch (message.Trim())
                    {
                        case "/exit":
                            operationCancelled = true;
                            break;
                        
                        case "/debug":
                            string json = JsonConvert.SerializeObject(_worldBuilder.SettingInfo);
                            AnsiConsole.Write(new JsonText(json));
                            break;
                        
                        case "/validate":
                            string validationInfo = _worldBuilder.SettingInfo!.Validate();
                            if (string.IsNullOrWhiteSpace(validationInfo))
                            {
                                AnsiConsole.MarkupLine("[Green]Setting is valid[/]");
                            }
                            else
                            {
                                AnsiConsole.MarkupLineInterpolated($"[Red]Setting is invalid[/]: {validationInfo}");
                            }
                            break;
                        
                        case "/create":
                            string result = _worldBuilder.SettingPlugin.BeginAdventure();
                            AnsiConsole.MarkupLine(result);
                            break;
                        
                        case "/help":
                            AnsiConsole.MarkupLine("[Yellow]Valid Commands[/]:");
                            AnsiConsole.MarkupLine("[Yellow]/debug[/]: Show debug information");
                            AnsiConsole.MarkupLine("[Yellow]/validate[/]: Validate the current setting information");
                            AnsiConsole.MarkupLine("[Yellow]/create[/]: Finalize the setting information and create the world");
                            AnsiConsole.MarkupLine("[Yellow]/exit[/]: Exit the world builder");
                            break;
                        
                        default:
                            AnsiConsole.MarkupLineInterpolated($"[Red]Unknown command[/]: {message}");
                            break;
                    }
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
                string key =
                    setting.CampaignName.Replace(" ", ""); // TODO: Check for restricted characters on blob names
                
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