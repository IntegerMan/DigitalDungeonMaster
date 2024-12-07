using MattEland.DigitalDungeonMaster.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class MainMenu
{
    private readonly RequestContextService _context;
    private readonly StorageDataService _storageDataService;
    private readonly LoadGameMenu _loadGameMenu;
    private readonly NewGameMenu _newGameMenu;

    public MainMenu(RequestContextService context, StorageDataService storageDataService, LoadGameMenu loadGameMenu, NewGameMenu newGameMenu)
    {
        _context = context;
        _storageDataService = storageDataService;
        _loadGameMenu = loadGameMenu;
        _newGameMenu = newGameMenu;
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
                return await _loadGameMenu.RunAsync();
            case newAdventure:
                return await _newGameMenu.RunAsync();
            case logout:
                _context.Logout();
                return true;
            case exit:
            default:
                return false;
        }
    }
}