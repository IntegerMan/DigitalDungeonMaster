using MattEland.BasementsAndBasilisks;
using MattEland.BasementsAndBasilisks.ConsoleApp;
using MattEland.BasementsAndBasilisks.ConsoleApp.Menus;
using MattEland.BasementsAndBasilisks.Models;
using MattEland.BasementsAndBasilisks.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

string appLogPath = Path.Combine(Environment.CurrentDirectory, "BasiliskApp.log");
string kernelLogPath = Path.Combine(Environment.CurrentDirectory, "BasiliskKernel.json");

// This log format is intended to be easily consumable as a transcript
await using Logger logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.File(path: appLogPath, outputTemplate: "{Message:lj}{NewLine}{Exception}{NewLine}")
    .CreateLogger();

try
{
    // Using UTF8 allows more capabilities for Spectre.Console.
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;

    logger.Information("Session Start");

    // Display the header
    DisplayHelpers.RenderHeader();

    AnsiConsole.MarkupLineInterpolated($"Logs and transcripts will be written to [Yellow]{Environment.CurrentDirectory}[/].");
    AnsiConsole.WriteLine();

    IServiceProvider serviceProvider = RegisterServices(kernelLogPath);

    LoginMenu loginMenu = serviceProvider.GetRequiredService<LoginMenu>();
    string? username = await loginMenu.RunAsync();

    if (username is not null)
    {
        LoadGameMenu loadGameMenu = serviceProvider.GetRequiredService<LoadGameMenu>();
        AdventureInfo? adventure = await loadGameMenu.RunAsync();

        if (adventure is not null)
        {

            // Set up our kernel and send an initial prompt if one is configured for this game
            using BasiliskKernel kernel = serviceProvider.GetRequiredService<BasiliskKernel>();
            await AnsiConsole.Status().StartAsync("Initializing the Game Master...",
                async _ =>
                {
                    ChatResult result = await kernel.InitializeKernelAsync(serviceProvider);
                    result.Blocks.Render();
                });

            // This loop lets the user interact with the kernel until they end the session
            await RunMainLoopAsync(kernel);
        }
    }

    DisplayHelpers.SayDungeonMasterLine("Goodbye, Adventurer!");
    logger.Information("Session End");
}
catch (Exception ex)
{
    logger.Fatal(ex, "An unhandled exception of type {Type} occurred in the main loop: {Message}",
        ex.GetType().FullName, ex.Message);
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
}

BasiliskConfig ReadConfiguration()
{
    // Read the configuration from appsettings.json and user secrets
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddUserSecrets<Program>()
        .Build();

    // Get the BasiliskConfig from the configuration using Binder
    BasiliskConfig config = new();
    configuration.Bind(config);

    return config;
}

async Task RunMainLoopAsync(BasiliskKernel kernel)
{
    do
    {
        AnsiConsole.WriteLine();
        string prompt = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]Player[/]: "));

        logger.Information("> {Message}", prompt);

        prompt = prompt.Trim();

        if (string.IsNullOrWhiteSpace(prompt)
            || prompt.Equals("exit", StringComparison.CurrentCultureIgnoreCase)
            || prompt.Equals("quit", StringComparison.CurrentCultureIgnoreCase)
            || prompt.Equals("goodbye", StringComparison.CurrentCultureIgnoreCase)
            || prompt.Equals("q", StringComparison.CurrentCultureIgnoreCase)
            || prompt.Equals("x", StringComparison.CurrentCultureIgnoreCase)
            || prompt.Equals("bye", StringComparison.CurrentCultureIgnoreCase))
        {
            break;
        }

        await ChatWithKernelAsync(kernel, prompt, logger);
    } while (true);
}

IServiceProvider RegisterServices(string logPath)
{
    BasiliskConfig config = ReadConfiguration();

    ServiceCollection collection = new();

    collection.AddScoped<BasiliskConfig>(_ => config);
    collection.RegisterBasiliskServices();
    collection.RegisterBasiliskPlugins();
    
    collection.AddScoped<LoadGameMenu>();
    collection.AddScoped<LoginMenu>();
    
    BasiliskKernel kernel = new(collection, config, logPath);
    collection.AddScoped<BasiliskKernel>(_ => kernel);

    return collection.BuildServiceProvider();
}

async Task ChatWithKernelAsync(BasiliskKernel kernel, string prompt, Logger responseLogger)
{
    ChatResult? response = null;
    await AnsiConsole.Status().StartAsync("The Game Master is thinking...",
        async _ => { response = await kernel.ChatAsync(prompt); });

    responseLogger.Information("{Message}", response!.Message);
    response.Blocks.Render();
}

