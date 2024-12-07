using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.ConsoleApp.Menus;

public class LoginMenu
{
    private readonly RequestContextService _context;
    private readonly UserService _userService;

    public LoginMenu(RequestContextService context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }
    public async Task<string?> RunAsync()
    {
        string choiceLogin = "Login";
        string choiceCreateAccount = "Create Account";
        string choiceExit = "Exit";
        
        string choice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Select an option")
            .AddChoices(choiceLogin, choiceCreateAccount, choiceExit));
        
        if (choice == choiceExit)
        {
            return null;
        } else if (choice == choiceCreateAccount)
        {
            return await CreateAccountAsync();
        } else
        {
            return await LoginAsync();
        }
    }

    private async Task<string?> CreateAccountAsync()
    {
        string username = AnsiConsole.Prompt(new TextPrompt<string>("Enter your username:")).ToLowerInvariant();
        string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter your password:").Secret('*'));
        string password2 = AnsiConsole.Prompt(new TextPrompt<string>("Enter password again:").Secret('*'));
        
        // Verify passwords match
        if (password != password2)
        {
            AnsiConsole.MarkupLine("[Red]Passwords do not match. Please try again.[/]");
            return null;
        }
        
        // Store the salt and hash
        try
        {
            await _userService.RegisterAsync(username, password);
            
            _context.CurrentUser = username;
            AnsiConsole.MarkupLine($"[Green]Account created successfully. Welcome, {username}![/]");

            return username;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[Red]Failed to create account: {ex.Message}[/]");
            return null;
        }
    }

    private async Task<string?> LoginAsync()
    {
        string username = AnsiConsole.Prompt(new TextPrompt<string>("Enter your username:")).ToLowerInvariant();
        string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter your password:").Secret('*'));
        
        bool loginSuccess = await _userService.LoginAsync(username, password);

        if (loginSuccess)
        {
            _context.CurrentUser = username;
            AnsiConsole.MarkupLine($"[Green]Welcome back, {username}![/]");
            return username;
        }
        else
        {
            AnsiConsole.MarkupLine($"[Red]Could not log in. Check your username and password and try again.[/]");
            return null;
        }
    }
}