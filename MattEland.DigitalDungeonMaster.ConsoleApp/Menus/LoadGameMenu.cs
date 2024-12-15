using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class LoadGameMenu
{
    private readonly RequestContextService _context;
    private readonly ApiClient _client;

    public LoadGameMenu(RequestContextService context, ApiClient client)
    {
        _context = context;
        _client = client;
    }
    
    public async Task<AdventureInfo?> RunAsync()
    {
        List<AdventureInfo> adventures = [];
        await AnsiConsole.Status().StartAsync($"Fetching adventures...",
            async _ =>
            {
                adventures.AddRange(await _client.LoadAdventuresAsync());
            });

        _context.Blocks.Render();
        _context.ClearBlocks();
    
        if (!adventures.Any())
        {
            AnsiConsole.MarkupLine("[Red]No adventures found for this user. Please create an adventure first.[/]");
            return null;
        }

        AdventureInfo cancel = new()
        {
            Name = "Cancel"
        };
        adventures.Add(cancel);

        AdventureInfo adventure = AnsiConsole.Prompt(new SelectionPrompt<AdventureInfo>()
            .Title("Select an adventure")
            .AddChoices(adventures)
            .UseConverter(a => a.Name + (a == cancel ? string.Empty : $" ({a.Ruleset})")));

        if (adventure == cancel)
        {
            return null;
        }
    
        _context.CurrentAdventure = adventure;
        
        AnsiConsole.MarkupLineInterpolated($"Selected Adventure: [Yellow]{adventure.Name}[/], Ruleset: [Yellow]{adventure.Ruleset}[/]");
        return adventure;
    }
}