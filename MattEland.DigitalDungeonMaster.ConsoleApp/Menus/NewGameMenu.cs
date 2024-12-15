using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class NewGameMenu
{
    private readonly RequestContextService _context;
    private readonly ApiClient _client;

    public NewGameMenu(RequestContextService context, ApiClient client)
    {
        _context = context;
        _client = client;
    }

    public async Task<bool> RunAsync()
    {
        List<Ruleset> rulesets = [];
        await AnsiConsole.Status().StartAsync("Loading rulesets...",
            async _ => rulesets.AddRange(await _client.LoadRulesetsAsync(_context.CurrentUser!)));

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
                async _ => response = await _client.StartWorldBuilderConversationAsync());

            // TODO: response!.Blocks.Render();
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
                else
                {
                    await AnsiConsole.Status().StartAsync("Waiting for world builder...",
                        async _ => response = await _client.ChatWithWorldBuilderAsync(new ChatRequest
                        {
                            Message = message
                        }));

                        // TODO: response?.Blocks.Render();
                }
            } while (!operationCancelled); // TODO: Some form of game management is needed here

            /*
            NewGameSettingInfo? setting = null; // TODO: _worldBuilder.SettingInfo;

            // Create the adventure
            if (!operationCancelled && setting is not null)
            {
                await AnsiConsole.Status().StartAsync("Creating adventure...",
                    async _ => await _client.CreateAdventureAsync(setting, ruleset.Key));
            }
            */
        }

        return true;
    }
}