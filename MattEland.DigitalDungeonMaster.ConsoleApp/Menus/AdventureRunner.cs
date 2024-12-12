using MattEland.DigitalDungeonMaster.Agents.GameMaster;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class AdventureRunner
{
    private readonly GameMasterAgent _gm;
    private readonly IServiceProvider _serviceProvider;
    private readonly RequestContextService _context;
    private readonly ILogger<AdventureRunner> _logger;

    public AdventureRunner(GameMasterAgent gm, 
        IServiceProvider serviceProvider, 
        ILogger<AdventureRunner> logger, 
        RequestContextService context)
    {
        _gm = gm;
        _serviceProvider = serviceProvider;
        _context = context;
        _logger = logger;
    }
    
    public async Task<bool> RunAsync(bool isNewAdventure)
    {
        _logger.LogDebug("Session Start");
        
        await AnsiConsole.Status().StartAsync("Initializing the Game Master...",
            async _ =>
            {
                _gm.IsNewAdventure = isNewAdventure;
                ChatResult result = await _gm.InitializeAsync(_serviceProvider);
                result.Blocks.Render();
            });

        // This loop lets the user interact with the kernel until they end the session
        await RunMainLoopAsync();

        _logger.LogDebug("Session End");
        
        return true;
    }
    
    private async Task RunMainLoopAsync()
    {
        do
        {
            AnsiConsole.WriteLine();
            string prompt = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]Player[/]: "));

            prompt = prompt.Trim();

            if (string.IsNullOrWhiteSpace(prompt)
                || prompt.Equals("exit", StringComparison.CurrentCultureIgnoreCase)
                || prompt.Equals("quit", StringComparison.CurrentCultureIgnoreCase)
                || prompt.Equals("goodbye", StringComparison.CurrentCultureIgnoreCase)
                || prompt.Equals("q", StringComparison.CurrentCultureIgnoreCase)
                || prompt.Equals("x", StringComparison.CurrentCultureIgnoreCase)
                || prompt.Equals("bye", StringComparison.CurrentCultureIgnoreCase))
            {
                _context.CurrentAdventure = null;
            }
            else
            {
                _logger.LogInformation("> {Message}", prompt);
                
                await ChatWithKernelAsync(prompt);
            }
        } while (_context.CurrentAdventure is not null);
    }
    
    private async Task ChatWithKernelAsync(string userMessage)
    {
        ChatResult? response = null;
        await AnsiConsole.Status().StartAsync("The Game Master is thinking...",
            async _ => { response = await _gm.ChatAsync(new ChatRequest
            {
                Message = userMessage
            }); 
        });

        _logger.LogInformation("{Message}", response!.Message);
        response.Blocks.Render();
    }
}