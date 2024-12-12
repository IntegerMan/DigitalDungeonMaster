using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class LoginMenu
{
    private readonly RequestContextService _context;
    private readonly UserService _userService;
    private readonly IOptionsSnapshot<UserSavedInfo> _userInfo;

    public LoginMenu(RequestContextService context, UserService userService, IOptionsSnapshot<UserSavedInfo> userInfo)
    {
        _context = context;
        _userService = userService;
        _userInfo = userInfo;
    }
    
    public async Task<bool> RunAsync()
    {
        // Support saved credentials
        if (!string.IsNullOrWhiteSpace(_userInfo.Value.Username) && _userInfo.Value.PasswordHash is not null)
        {
            byte[] passwordHash = Convert.FromBase64String(_userInfo.Value.PasswordHash);
            bool loginSuccess = false;
            await AnsiConsole.Status().StartAsync("Logging in with saved credentials...",
                async _ => loginSuccess = await _userService.LoginAsync(_userInfo.Value.Username, passwordHash));

            if (!loginSuccess)
            {
                AnsiConsole.MarkupLine("[Red]Failed to log in. Please log in manually.[/]");
            }
            else
            {
                HandleLoginSuccess(_userInfo.Value.Username);
                return true;
            }
        }
        
        const string choiceLogin = "Login";
        const string choiceCreateAccount = "Create Account";
        const string choiceExit = "Exit";
        
        string choice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Select an option")
            .AddChoices(choiceLogin, choiceCreateAccount, choiceExit));

        switch (choice)
        {
            case choiceCreateAccount:
                await CreateAccountAsync();
                return true;
            case choiceLogin:
                await LoginAsync();
                return true;
            default:
                return false;
        }
    }

    private async Task CreateAccountAsync()
    {
        string username = AnsiConsole.Prompt(new TextPrompt<string>("Enter your username:")).ToLowerInvariant();
        string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter your password:").Secret('*'));
        string password2 = AnsiConsole.Prompt(new TextPrompt<string>("Enter password again:").Secret('*'));
        
        // Verify passwords match
        if (password != password2)
        {
            AnsiConsole.MarkupLine("[Red]Passwords do not match. Please try again.[/]");
            return;
        }
        
        // Store the salt and hash
        try
        {
            await AnsiConsole.Status().StartAsync("Creating account...",
                async _ => await _userService.RegisterAsync(username, password));
            
            _context.CurrentUser = username;
            AnsiConsole.MarkupLine($"[Green]Account created successfully. Welcome, {username}![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[Red]Failed to create account: {ex.Message}[/]");
        }
    }

    private async Task LoginAsync()
    {
        string username = AnsiConsole.Prompt(new TextPrompt<string>("Enter your username:")).ToLowerInvariant();
        string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter your password:").Secret('*'));

        bool loginSuccess = false;
        
        await AnsiConsole.Status().StartAsync("Logging in...",
            async _ => loginSuccess = await _userService.LoginAsync(username, password));

        if (loginSuccess)
        {
            HandleLoginSuccess(username);
            
            // We could save this locally if we wanted to, though user secrets is a workaround for development
        }
        else
        {
            AnsiConsole.MarkupLine($"[Red]Could not log in. Check your username and password and try again.[/]");
        }
    }

    private void HandleLoginSuccess(string username)
    {
        _context.CurrentUser = username;
        AnsiConsole.MarkupLine($"[Green]Welcome back, {username}![/]");
    }
}