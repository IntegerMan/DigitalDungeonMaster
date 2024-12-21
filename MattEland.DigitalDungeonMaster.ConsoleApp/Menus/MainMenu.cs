using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class MainMenu
{
    private readonly LoadGameMenu _loadGameMenu;
    private readonly NewGameMenu _newGameMenu;
    private readonly ApiClient _client;

    public MainMenu(LoadGameMenu loadGameMenu, NewGameMenu newGameMenu, ApiClient client)
    {
        _loadGameMenu = loadGameMenu;
        _newGameMenu = newGameMenu;
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