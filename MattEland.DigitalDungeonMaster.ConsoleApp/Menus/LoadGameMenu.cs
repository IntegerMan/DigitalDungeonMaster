using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.GameManagement.Services;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class LoadGameMenu
{
    private readonly RequestContextService _context;
    private readonly AdventuresService _adventureService;

    public LoadGameMenu(RequestContextService context, AdventuresService adventureService)
    {
        _context = context;
        _adventureService = adventureService;
    }
    
    public async Task<bool> RunAsync()
    {
        string user = _context.CurrentUser ?? throw new InvalidOperationException("Current user is not set");
        List<AdventureInfo> adventures = [];
        await AnsiConsole.Status().StartAsync($"Fetching adventures for {user} ...",
            async _ => { adventures.AddRange(await _adventureService.LoadAdventuresAsync(user)); });

        _context.Blocks.Render();
        _context.ClearBlocks();
    
        if (!adventures.Any())
        {
            AnsiConsole.MarkupLine("[Red]No adventures found for this user. Please create an adventure first.[/]");
            return true;
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
            return true;
        }
    
        _context.CurrentAdventure = adventure;
        
        AnsiConsole.MarkupLineInterpolated($"Selected Adventure: [Yellow]{adventure.Name}[/], Ruleset: [Yellow]{adventure.Ruleset}[/]");
        return true;
    }
}