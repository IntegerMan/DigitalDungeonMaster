using MattEland.DigitalDungeonMaster.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class LoadGameMenu
{
    private readonly StorageDataService _dataService;
    private readonly RequestContextService _context;

    public LoadGameMenu(StorageDataService dataService, RequestContextService context)
    {
        _dataService = dataService;
        _context = context;
    }
    
    public async Task<AdventureInfo?> RunAsync()
    {
        string user = _context.CurrentUser ?? throw new InvalidOperationException("Current user is not set");
        List<AdventureInfo> adventures = [];
        await AnsiConsole.Status().StartAsync($"Fetching adventures for {user} ...",
            async _ => { adventures.AddRange(await _dataService.LoadAdventuresAsync(user)); });

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