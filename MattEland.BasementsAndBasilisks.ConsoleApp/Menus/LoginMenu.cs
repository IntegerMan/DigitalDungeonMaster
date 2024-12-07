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
        string? username = AnsiConsole.Prompt(new TextPrompt<string>("Enter your username").DefaultValue("meland"));

        bool exists = await _userService.UserExistsAsync(username);

        if (exists)
        {
            _context.CurrentUser = username;
            AnsiConsole.MarkupLine($"[Green]Welcome back, {username}![/]");
            return username;
        }
        else
        {
            AnsiConsole.MarkupLine($"[Red]User {username} does not exist. Please create an account first.[/]");
            return null;
        }
    }
}