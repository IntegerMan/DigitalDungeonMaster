using MattEland.BasementsAndBasilisks;
using MattEland.BasementsAndBasilisks.ConsoleApp;
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
    AnsiConsole.Write(new FigletText("Basements & Basilisks").Color(Color.Yellow));
    AnsiConsole.MarkupLine("AI Orchestration proof of concept by [SteelBlue]Matt Eland[/].");
    AnsiConsole.WriteLine();

    AnsiConsole.MarkupLineInterpolated($"Application logging to [Yellow]{appLogPath}[/].");
    AnsiConsole.MarkupLineInterpolated($"Kernel logging to [Yellow]{kernelLogPath}[/].");
    AnsiConsole.WriteLine();

    IServiceProvider serviceProvider = RegisterServices(kernelLogPath);
    using BasiliskKernel kernel = serviceProvider.GetRequiredService<BasiliskKernel>();

    // TODO: This should probably come from game information
    string prompt = """
Hello, Dungeon Master! Please greet me with a recap of our last session and ask me what my goals are for this session. 
Once you have these, ask me what I'd like to do.
""";

    logger.Information("Generating story recap: {Prompt}", prompt);
    
    await ChatWithKernelAsync(kernel, prompt, logger);
    await RunMainLoopAsync(kernel);

    DisplayHelpers.SayDungeonMasterLine("Goodbye, Adventurer!");
    logger.Information("Session End");
}
catch (Exception ex)
{
    logger.Fatal(ex, "An unhandled exception of type {Type} occurred in the main loop: {Message}", ex.GetType().FullName, ex.Message);
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
    await AnsiConsole.Status().StartAsync("Waiting for Game Master...", async _ =>
    {
        response = await kernel.ChatAsync(prompt);
    });
    
    responseLogger.Information("{Message}", response!.Message);
    response.Blocks.Render();
}

