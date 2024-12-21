using MattEland.DigitalDungeonMaster.ClientShared;
using MattEland.DigitalDungeonMaster.ConsoleApp.Settings;
using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class LoginMenu
{
    private readonly ApiClient _client;
    private readonly UserSavedInfo _userInfo;

    public LoginMenu(IOptionsSnapshot<UserSavedInfo> userInfo, 
        ApiClient client)
    {
        _client = client;
        _userInfo = userInfo.Value;
    }
    
    public async Task<bool> RunAsync()
    {
        // Support saved credentials
        /*
        if (!string.IsNullOrWhiteSpace(_userInfo.Username) && _userInfo.PasswordHash is not null)
        {
            byte[] passwordHash = Convert.FromBase64String(_userInfo.PasswordHash);
            bool loginSuccess = false;
            await AnsiConsole.Status().StartAsync("Logging in with saved credentials...",
                async _ => loginSuccess = await _userService.LoginAsync(_userInfo.Username, passwordHash));

            if (!loginSuccess)
            {
                AnsiConsole.MarkupLine("[Red]Failed to log in. Please log in manually.[/]");
            }
            else
            {
                HandleLoginSuccess(_userInfo.Username);
                return true;
            }
        }
        */
        
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
        
        // Register
        ApiResult result = await AnsiConsole.Status().StartAsync("Creating account...",
            async _ => await _client.RegisterAsync(username, password));

        if (result.Success)
        {
            AnsiConsole.MarkupLine($"[Green]Account created successfully. Welcome, {username}![/]");
        }
        else
        {
            AnsiConsole.MarkupLineInterpolated($"[Red]{result.ErrorMessage}[/]");
        }
    }

    private async Task LoginAsync()
    {
        string username = AnsiConsole.Prompt(new TextPrompt<string>("Enter your username:")).ToLowerInvariant();
        string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter your password:").Secret('*'));

        ApiResult? result = null;
        await AnsiConsole.Status().StartAsync("Logging in...",
            async _ =>
            {
                result = await _client.LoginAsync(username, password);
            });

        if (result is { Success: true })
        {
            AnsiConsole.MarkupLine($"[Green]Welcome back, {username}![/]");
        }
        else
        {
            AnsiConsole.MarkupLineInterpolated($"[Red]{result?.ErrorMessage}[/]");
        }
    }
}