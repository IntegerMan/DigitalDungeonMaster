using MattEland.BasementsAndBasilisks;
using MattEland.BasementsAndBasilisks.ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

try
{
    // Using UTF8 allows more capabilities for Spectre.Console.
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;

    // Display the header
    AnsiConsole.Write(new FigletText("Basements & Basilisks").Color(Color.Yellow));
    AnsiConsole.MarkupLine("AI Orchestration proof of concept by [SteelBlue]Matt Eland[/].");
    AnsiConsole.WriteLine();

    IServiceProvider serviceProvider = RegisterServices();
    using BasiliskKernel kernel = serviceProvider.GetRequiredService<BasiliskKernel>();

    string prompt = """
Hello, Dungeon Master! Please greet me with a recap of our last session and ask me what my goals are for this session. 
Once you have these, ask me what I'd like to do.
""";

    string response = await kernel.ChatAsync(prompt);
    SayDungeonMasterLine(response);

    await RunMainLoopAsync(kernel);

    SayDungeonMasterLine("Goodbye, Adventurer!");
}
catch (Exception ex)
{
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
    // TODO: This really should write to a file as well as to the console
    string response;
    do
    {
        AnsiConsole.WriteLine();
        string message = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]Player[/]: "))! ?? string.Empty;

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
            SayDungeonMasterLine(response);
        }
        else
        {
            break;
        }
    } while (true);
}

IServiceProvider RegisterServices()
{
    BasiliskConfig config = ReadConfiguration();

    ServiceCollection collection = new();

    collection.RegisterBasiliskServices();
    collection.RegisterBasiliskPlugins();

    collection.AddScoped<BasiliskKernel>(s => new(s, config.AzureOpenAiDeploymentName,
        config.AzureOpenAiEndpoint,
        config.AzureOpenAiKey));

    return collection.BuildServiceProvider();
}

static void SayDungeonMasterLine(string response)
{
    Console.WriteLine();
    try
    {
        AnsiConsole.MarkupLine("[SteelBlue]DM[/]: " + response);
    }
    catch
    {
        // Fallback to writeline in cases where response is not valid markup
        AnsiConsole.WriteLine($"DM: {response}");
    }
}