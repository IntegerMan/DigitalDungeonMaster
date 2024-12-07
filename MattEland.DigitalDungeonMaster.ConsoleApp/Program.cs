using MattEland.DigitalDungeonMaster;
using MattEland.DigitalDungeonMaster.ConsoleApp;
using MattEland.DigitalDungeonMaster.ConsoleApp.Menus;
using MattEland.DigitalDungeonMaster.Models;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

string appLogPath = Path.Combine(Environment.CurrentDirectory, "Session.log");
string kernelLogPath = Path.Combine(Environment.CurrentDirectory, "Kernel.json");

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
    MainMenu mainMenu = serviceProvider.GetRequiredService<MainMenu>();
    AdventureRunner adventureRunner = serviceProvider.GetRequiredService<AdventureRunner>();
    RequestContextService context = serviceProvider.GetRequiredService<RequestContextService>();

    bool keepGoing = true;
    do
    {
        if (context.CurrentUser is null)
        {
            keepGoing = await loginMenu.RunAsync();
        }
        
        if (context.CurrentUser is not null)
        {
            keepGoing = await mainMenu.RunAsync();

            if (context.CurrentAdventure is not null)
            {
                keepGoing = await adventureRunner.RunAsync();
            }
        }
    }
    while (keepGoing);

    DisplayHelpers.SayDungeonMasterLine("Goodbye, Adventurer!");
    logger.Information("Session End");
}
catch (Exception ex)
{
    logger.Fatal(ex, "An unhandled exception of type {Type} occurred in the main loop: {Message}",
        ex.GetType().FullName, ex.Message);
    
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
}

AzureResourceConfig ReadConfiguration()
{
    // Read the configuration from appsettings.json and user secrets
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddUserSecrets<Program>()
        .Build();

    // Get the config from the configuration  using Binder
    AzureResourceConfig config = new();
    configuration.Bind(config);

    return config;
}

IServiceProvider RegisterServices(string logPath)
{
    ServiceCollection collection = new();
    
    // Configure logging
    collection.AddScoped<LoggerFactory>();
    
    // Front-end menus
    collection.AddScoped<LoadGameMenu>();
    collection.AddScoped<LoginMenu>();
    collection.AddScoped<MainMenu>();
    collection.AddScoped<AdventureRunner>();
    
    // Configure the kernel
    AzureResourceConfig config = ReadConfiguration();
    MainKernel kernel = new(collection, config, logPath);
    collection.AddScoped<MainKernel>(_ => kernel);
    collection.AddScoped<AzureResourceConfig>(_ => config);
    collection.RegisterGameServices();
    collection.RegisterGamePlugins();

    return collection.BuildServiceProvider();
}

