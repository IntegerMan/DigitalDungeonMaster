using MattEland.DigitalDungeonMaster.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class MainMenu
{
    private readonly RequestContextService _context;
    private readonly StorageDataService _storageDataService;
    private readonly LoadGameMenu _loadGameMenu;

    public MainMenu(RequestContextService context, StorageDataService storageDataService, LoadGameMenu loadGameMenu)
    {
        _context = context;
        _storageDataService = storageDataService;
        _loadGameMenu = loadGameMenu;
    }
    
    public async Task<bool> RunAsync()
    {
        const string continueAdventure = "Continue Adventure";
        const string newAdventure = "New Adventure";
        const string logout = "Logout";
        const string exit = "Exit";
        
        string choice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Main Menu")
            .AddChoices("Continue Adventure", "New Adventure", "Logout", "Exit"));
        
        switch (choice)
        {
            case continueAdventure:
                await _loadGameMenu.RunAsync();
                return true;
            case newAdventure:
                await NewAdventureAsync();
                return true;
            case logout:
                _context.Logout();
                return true;
            default:
                return false;
        }
    }

    private async Task NewAdventureAsync()
    {
        string adventureName = AnsiConsole.Prompt(new TextPrompt<string>("Enter the name of your new adventure:"));
        string key = adventureName.Replace(" ", string.Empty).ToLowerInvariant();
        
        string description = AnsiConsole.Prompt(new TextPrompt<string>("Enter a description for your adventure:"));
        
        string ruleset = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select the ruleset for your new adventure:")
            .AddChoices("dnd5e", "GURPS", "Traveler"));
        
        AdventureInfo adventure = new()
        {
            Name = adventureName,
            Ruleset = ruleset,
            Description = description,
            Container = $"{_context.CurrentUser!}_{key}",
            RowKey = adventureName
        };
        
        // TODO: Actually create the adventure on the server
        
        _context.CurrentAdventure = adventure;

        await Task.CompletedTask;
    }
}