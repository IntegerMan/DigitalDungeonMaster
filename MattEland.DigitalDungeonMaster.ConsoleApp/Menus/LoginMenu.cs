using System.Net.Http.Headers;
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
    private readonly ILogger<LoginMenu> _logger;
    private readonly UserSavedInfo _userInfo;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ServerSettings _serverSettings;

    public LoginMenu(RequestContextService context, 
        ILogger<LoginMenu> logger,
        IOptionsSnapshot<UserSavedInfo> userInfo, 
        IHttpClientFactory clientFactory, 
        IOptionsSnapshot<ServerSettings> serverSettings)
    {
        _context = context;
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
        
        // Register
        string errorMessage = "Failed to create account. Please try again.";
        bool registerSuccess = await AnsiConsole.Status().StartAsync("Creating account...",
            async _ =>
            {
                Uri uri = new Uri(_serverSettings.BaseUrl + "register");
                _logger.LogDebug("Registering at {Uri} as {Username}", uri, username);
                using HttpClient client = _clientFactory.CreateClient();
                try
                {
                    HttpResponseMessage response = await client.PostAsync(uri, CreateJsonContent(new
                    {
                        Username = username,
                        Password = password
                    }));

                    _logger.LogDebug("Register response: {Response}", response);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Registered successfully as user {Username}", username);
                        // TODO: In the future we'll want to get back a JWT

                        return true;
                    }

                    _logger.LogWarning("Failed to register user {Username}", username);
                    errorMessage = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage = "Failed to create account. Server returned status code " + response.StatusCode + " for " + uri;
                    }
                }
                catch (HttpRequestException ex)
                {
                    errorMessage = "Network error occurred trying to register user. Please try again.";
                    _logger.LogError(ex, "Network error occurred trying to register user {Username}", username);
                }                
                catch (TaskCanceledException ex)
                {
                    errorMessage = "Timed out trying to register user. Please try again.";
                    _logger.LogError(ex, "Timed out trying to register user {Username}", username);
                }

                return false;
            });

        if (registerSuccess)
        {
            _context.CurrentUser = username;
            AnsiConsole.MarkupLine($"[Green]Account created successfully. Welcome, {username}![/]");
        }
        else
        {
            AnsiConsole.MarkupLineInterpolated($"[Red]{errorMessage}[/]");
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
                Uri uri = new Uri(_serverSettings.BaseUrl + "login");
                _logger.LogDebug("Logging in to {Uri} as {Username}", uri, username);
                using HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                try
                {
                    // Create HttpContent with JSON Content for the user
                    HttpResponseMessage response = await client.PostAsync(uri, CreateJsonContent(new
                    {
                        Username = username,
                        Password = password
                    }));

                    _logger.LogDebug("Login response: {Response}", response);

                    if (response.IsSuccessStatusCode)
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

    private static HttpContent CreateJsonContent(object payload)
    {
        HttpContent content = new StringContent(JsonConvert.SerializeObject(payload));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        return content;
    }

    private void HandleLoginSuccess(string username)
    {
        _context.CurrentUser = username;
        
        AnsiConsole.MarkupLine($"[Green]Welcome back, {username}![/]");
    }
}