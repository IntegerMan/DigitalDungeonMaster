using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.Menus;

public class AdventureRunner
{
    private readonly MainKernel _kernel;
    private readonly IServiceProvider _serviceProvider;
    private readonly RequestContextService _context;
    private readonly ILogger<AdventureRunner> _logger;

    public AdventureRunner(MainKernel kernel, 
        IServiceProvider serviceProvider, 
        ILoggerFactory loggerFactory, 
        RequestContextService context)
    {
        _kernel = kernel;
        _serviceProvider = serviceProvider;
        _context = context;
        _logger = loggerFactory.CreateLogger<AdventureRunner>();
    }
    
    public async Task<bool> RunAsync(bool isNewAdventure)
    {
        _logger.LogDebug("Session Start");
        
        await AnsiConsole.Status().StartAsync("Initializing the Game Master...",
            async _ =>
            {
                ChatResult result = await _kernel.InitializeKernelAsync(_serviceProvider, isNewAdventure);
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
    
    private async Task ChatWithKernelAsync(string prompt)
    {
        ChatResult? response = null;
        await AnsiConsole.Status().StartAsync("The Game Master is thinking...",
            async _ => { response = await _kernel.ChatAsync(prompt); });

        _logger.LogInformation("{Message}", response!.Message);
        response.Blocks.Render();
    }
}