namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class MainMenu
{
    private readonly LoadGameMenu _loadGameMenu;
    private readonly NewGameMenu _newGameMenu;

    public MainMenu(LoadGameMenu loadGameMenu, NewGameMenu newGameMenu)
    {
        _loadGameMenu = loadGameMenu;
        _newGameMenu = newGameMenu;
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
                break;
            
            case exit:
                keepGoing = false;
                break;
        }
        
        return (keepGoing, isNewAdventure);
    }
}