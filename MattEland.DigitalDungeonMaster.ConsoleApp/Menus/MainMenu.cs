using MattEland.DigitalDungeonMaster.GameManagement.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class MainMenu
{
    private readonly LoadGameMenu _loadGameMenu;
    private readonly NewGameMenu _newGameMenu;
    private readonly RequestContextService _context;
    private readonly ApiClient _client;

    public MainMenu(LoadGameMenu loadGameMenu, NewGameMenu newGameMenu, RequestContextService context, ApiClient client)
    {
        _loadGameMenu = loadGameMenu;
        _newGameMenu = newGameMenu;
        _context = context;
        _client = client;
    }
    
    public async Task<(bool, AdventureInfo?)> RunAsync()
    {
        const string continueAdventure = "Continue Adventure";
        const string newAdventure = "New Adventure";
        const string logout = "Logout";
        const string exit = "Exit";
        
        bool keepGoing = true;
        AdventureInfo? adventure = null;
        
        string choice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Main Menu")
            .AddChoices("Continue Adventure", "New Adventure", "Logout", "Exit"));
        
        switch (choice)
        {
            case continueAdventure:
                adventure = await _loadGameMenu.RunAsync();
                break;
            
            case newAdventure:
                adventure = await _newGameMenu.RunAsync();
                break;
            
            case logout:
                _client.Logout();
                break;
            
            case exit:
                keepGoing = false;
                break;
        }
        
        return (keepGoing, adventure);
    }
}