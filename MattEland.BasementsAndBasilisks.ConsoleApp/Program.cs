using MattEland.BasementsAndBasilisks;
using MattEland.BasementsAndBasilisks.ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

string appLogPath = Path.Combine(Environment.CurrentDirectory, "BasiliskApp.log");
string kernelLogPath = Path.Combine(Environment.CurrentDirectory, "BasiliskKernel.json");

// This log format is intended to be easily consumable as a transcript
await using Serilog.Core.Logger logger = new LoggerConfiguration()
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

    string prompt = """
Hello, Dungeon Master! Please greet me with a recap of our last session and ask me what my goals are for this session. 
Once you have these, ask me what I'd like to do.
""";

    logger.Information("Generating story recap: {Prompt}", prompt);

    // TODO: This would be better UX if we used the status indicator

    ChatResult response = await kernel.ChatAsync(prompt);

    DisplayHelpers.SayDungeonMasterLine(response, logger);

    await RunMainLoopAsync(kernel);

    DisplayHelpers.SayDungeonMasterLine("Goodbye, Adventurer!", logger);
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

async Task RunMainLoopAsync(BasiliskKernel basiliskKernel)
{
    ChatResult response;
    do
    {
        AnsiConsole.WriteLine();
        string message = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]Player[/]: "))! ?? string.Empty;

        logger.Information("> {Message}", message);

        message = message.Trim();

        if (!string.IsNullOrWhiteSpace(message)
            && !message.Equals("exit", StringComparison.CurrentCultureIgnoreCase)
            && !message.Equals("quit", StringComparison.CurrentCultureIgnoreCase)
            && !message.Equals("goodbye", StringComparison.CurrentCultureIgnoreCase)
            && !message.Equals("q", StringComparison.CurrentCultureIgnoreCase)
            && !message.Equals("x", StringComparison.CurrentCultureIgnoreCase)
            && !message.Equals("bye", StringComparison.CurrentCultureIgnoreCase))
        {
            // Send the message to the BasiliskKernel and get a response
            response = await basiliskKernel.ChatAsync(message);

            // TODO: I'd like to handle rich content triggered by kernel actions here - things like stat blocks, diagnostics, etc.

            // Display the response
            DisplayHelpers.SayDungeonMasterLine(response, logger);
        }
        else
        {
            break;
        }
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

