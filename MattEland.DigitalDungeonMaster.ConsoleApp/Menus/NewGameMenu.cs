using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class NewGameMenu
{
    private readonly ApiClient _client;

    public NewGameMenu(ApiClient client)
    {
        _client = client;
    }

    public async Task<AdventureInfo?> RunAsync()
    {
        List<Ruleset> rulesets = [];
        await AnsiConsole.Status().StartAsync("Loading rulesets...",
            async _ => rulesets.AddRange(await _client.LoadRulesetsAsync()));

        if (!rulesets.Any())
        {
            AnsiConsole.MarkupLine("[Red]No rulesets found. Please create a ruleset first.[/]");
            return null;
        }

        // Select a ruleset
        rulesets.Add(new Ruleset { Name = "Cancel", Key = "Cancel", Owner = "System" });
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

            response.Render();
            
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
                            User = _client.Username,
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

        return null;
    }
}