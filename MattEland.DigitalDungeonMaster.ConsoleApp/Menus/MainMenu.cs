using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class MainMenu
{
    private readonly LoadGameMenu _loadGameMenu;
    private readonly NewGameMenu _newGameMenu;
    private readonly RequestContextService _context;

    public MainMenu(LoadGameMenu loadGameMenu, NewGameMenu newGameMenu, RequestContextService context)
    {
        _loadGameMenu = loadGameMenu;
        _newGameMenu = newGameMenu;
        _context = context;
    }
    
    public async Task<(bool, bool)> RunAsync()
    {
        const string continueAdventure = "Continue Adventure";
        const string newAdventure = "New Adventure";
        const string logout = "Logout";
        const string exit = "Exit";
        
        bool keepGoing = true;
        bool isNewAdventure = false;
        
        string choice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Main Menu")
            .AddChoices("Continue Adventure", "New Adventure", "Logout", "Exit"));
        
        switch (choice)
        {
            case continueAdventure:
                await _loadGameMenu.RunAsync();
                break;
            
            case newAdventure:
                await _newGameMenu.RunAsync();
                isNewAdventure = true;
                break;
            
            case logout:
                _context.Logout();
                break;
            
            case exit:
                keepGoing = false;
                break;
        }
        
        return (keepGoing, isNewAdventure);
    }
}