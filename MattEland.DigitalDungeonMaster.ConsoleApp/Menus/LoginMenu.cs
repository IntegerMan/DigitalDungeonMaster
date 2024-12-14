using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.GameManagement.Services;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class LoginMenu
{
    private readonly RequestContextService _context;
    private readonly UserService _userService;
    private readonly ILogger<LoginMenu> _logger;
    private readonly UserSavedInfo _userInfo;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ServerSettings _serverSettings;

    public LoginMenu(RequestContextService context, 
        UserService userService, 
        ILogger<LoginMenu> logger,
        IOptionsSnapshot<UserSavedInfo> userInfo, 
        IHttpClientFactory clientFactory, 
        IOptionsSnapshot<ServerSettings> serverSettings)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
        _userInfo = userInfo.Value;
        _clientFactory = clientFactory;
        _serverSettings = serverSettings.Value;
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
            _logger.LogError(ex, "Failed to create account for {Username}", username);
        }
    }

    private async Task LoginAsync()
    {
        string username = AnsiConsole.Prompt(new TextPrompt<string>("Enter your username:")).ToLowerInvariant();
        string password = AnsiConsole.Prompt(new TextPrompt<string>("Enter your password:").Secret('*'));

        bool loginSuccess = false;
        
        await AnsiConsole.Status().StartAsync("Logging in...",
            async _ =>
            {
                Uri uri = new Uri(_serverSettings.BaseUrl + "/login");
                _logger.LogDebug("Logging in to {Uri} as {Username}", uri, username);
                using HttpClient client = _clientFactory.CreateClient();
                try
                {
                    HttpResponseMessage postResult = await client.PostAsync(uri,
                        new StringContent(JsonConvert.SerializeObject(new
                        {
                            Username = username,
                            Password = password
                        })));

                    _logger.LogDebug("Login response: {Response}", postResult);

                    if (postResult.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Logged in successfully as user {Username}", username);
                        loginSuccess = true;
                        // TODO: In the future we'll want to get back a JWT
                    }
                    else
                    {
                        _logger.LogWarning("Failed to log in user {Username}", username);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Network error occurred trying to log in user {Username}", username);
                }                
                catch (TaskCanceledException ex)
                {
                    _logger.LogError(ex, "Timed out trying to log in user {Username}", username);
                }

                return loginSuccess;
            });

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