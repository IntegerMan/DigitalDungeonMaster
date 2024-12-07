using MattEland.DigitalDungeonMaster.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class NewGameMenu
{
    private readonly RequestContextService _context;
    private readonly RulesetService _rulesetService;

    public NewGameMenu(RequestContextService context, RulesetService rulesetService)
    {
        _context = context;
        _rulesetService = rulesetService;
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
        
        string adventureName = AnsiConsole.Prompt(new TextPrompt<string>("Enter the name of your new adventure:"));
        string key = adventureName.Replace(" ", string.Empty).ToLowerInvariant();
        
        string description = AnsiConsole.Prompt(new TextPrompt<string>("Enter a description for your adventure:"));
        
        // Select a ruleset
        rulesets.Add(new Ruleset { Name = "Cancel", Key = "Cancel", Owner = _context.CurrentUser!});
        Ruleset ruleset = AnsiConsole.Prompt(new SelectionPrompt<Ruleset>()
            .Title("Select the ruleset for your new adventure:")
            .AddChoices(rulesets)
            .UseConverter(r => r.Name));
        
        if (ruleset.Key != "Cancel")
        {
            AdventureInfo adventure = new()
            {
                Name = adventureName,
                Ruleset = ruleset.Key,
                Description = description,
                Container = $"{_context.CurrentUser!}_{key}",
                RowKey = adventureName
            };
        
            // TODO: Actually create the adventure on the server
        
            _context.CurrentAdventure = adventure;
        }

        return true;
    }
}