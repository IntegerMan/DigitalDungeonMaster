using System.Net.Mime;
using MattEland.BasementsAndBasilisks;
using MattEland.BasementsAndBasilisks.ConsoleApp;
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

    AnsiConsole.MarkupLineInterpolated(
        $"Logs and transcripts will be written to [Yellow]{Environment.CurrentDirectory}[/].");
    AnsiConsole.WriteLine();

    IServiceProvider serviceProvider = RegisterServices(kernelLogPath);

    StorageDataService storageDataService = serviceProvider.GetRequiredService<StorageDataService>();
    string username = AnsiConsole.Prompt(new TextPrompt<string>("Enter your username").DefaultValue("meland"));
    
    // TODO: Password auth and existence verification is a good move
    
    AdventureInfo? adventure = await SelectAnAdventureAsync(storageDataService, username);

    if (adventure != null)
    {
        AnsiConsole.MarkupLineInterpolated(
            $"Selected Adventure: [Yellow]{adventure.Name}[/], Ruleset: [Yellow]{adventure.Ruleset}[/], World: [Yellow]{adventure.GameWorld}[/]");

        // Set the adventure and user into the context service
        RequestContextService context = serviceProvider.GetRequiredService<RequestContextService>();
        context.CurrentAdventure = adventure;
        context.CurrentUser = username;

        using BasiliskKernel kernel = serviceProvider.GetRequiredService<BasiliskKernel>();

        // TODO: This should probably come from game information or an agents file
        string prompt = """
                        Hello, Dungeon Master! Please greet me with a recap of our last session and ask me what my goals are for this session. 
                        Once you have these, ask me what I'd like to do.
                        """;

        logger.Information("Generating story recap: {Prompt}", prompt);

        await ChatWithKernelAsync(kernel, prompt, logger);
        await RunMainLoopAsync(kernel);
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
        string prompt = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]Player[/]: "))! ?? string.Empty;

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

    collection.AddScoped<BasiliskKernel>(s => new(s, config.AzureOpenAiDeploymentName,
        config.AzureOpenAiEndpoint,
        config.AzureOpenAiKey,
        logPath));

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

async Task<AdventureInfo?> SelectAnAdventureAsync(StorageDataService dataService, string user)
{
    List<AdventureInfo> adventures = new List<AdventureInfo>();
    await AnsiConsole.Status().StartAsync("Fetching adventures...",
        async _ => { adventures.AddRange(await dataService.LoadAdventuresAsync(user)); });

    if (!adventures.Any())
    {
        AnsiConsole.MarkupLine("[Red]No adventures found for this user. Please create an adventure first.[/]");
        return null;
    }

    AdventureInfo empty = new AdventureInfo
    {
        Name = "Cancel",
        Description = "Cancel and exit the application",
        Container = "N/A",
        Ruleset = "N/A",
        GameWorld = "N/A",
        RowKey = "N/A"
    };
    adventures.Add(empty);

    AdventureInfo adventureInfo = AnsiConsole.Prompt(new SelectionPrompt<AdventureInfo>()
        .Title("Select an adventure")
        .AddChoices(adventures)
        .UseConverter(a => a.Name + (a == empty ? string.Empty : $" ({a.Ruleset})")));

    return adventureInfo == empty
        ? null
        : adventureInfo;
}