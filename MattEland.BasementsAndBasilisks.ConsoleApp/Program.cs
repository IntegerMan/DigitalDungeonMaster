using MattEland.BasementsAndBasilisks;
using MattEland.BasementsAndBasilisks.ConsoleApp;
using MattEland.BasementsAndBasilisks.Plugins;
using MattEland.BasementsAndBasilisks.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// TODO: This would be better if we used AnsiConsole from Spectre.Console

IServiceProvider serviceProvider = RegisterServices();
BasiliskKernel kernel = serviceProvider.GetRequiredService<BasiliskKernel>();

string response = await kernel.ChatAsync("Hello, Dungeon Master! Please greet me with a recap of our last session and ask me what my goals are for this session. Once you have these, ask me what I'd like to do.");
Console.WriteLine(response);

await RunMainLoopAsync(kernel);

Console.WriteLine("Goodbye, Adventurer!");

BasiliskConfig ReadConfiguration()
{
    // Read the configuration from appsettings.json and user secrets
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
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
        string message = Console.ReadLine()!;
        if (!string.IsNullOrWhiteSpace(message))
        {
            response = await basiliskKernel.ChatAsync(message);
            Console.WriteLine(response);
        } else {
            break;
        }
    } while (true);
}

IServiceProvider RegisterServices()
{
    BasiliskConfig config = ReadConfiguration();

    ServiceCollection collection = new();

    collection.AddScoped<RandomService>();
    collection.RegisterBasiliskPlugins();
    
    collection.AddScoped<BasiliskKernel>(s => new(s, config.AzureOpenAiDeploymentName,
        config.AzureOpenAiEndpoint,
        config.AzureOpenAiKey));

    return collection.BuildServiceProvider();
}