using MattEland.BasementsAndBasilisks;
using MattEland.BasementsAndBasilisks.ConsoleApp;
using MattEland.BasementsAndBasilisks.Plugins;
using MattEland.BasementsAndBasilisks.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

IServiceProvider serviceProvider = RegisterServices();
BasiliskKernel kernel = serviceProvider.GetRequiredService<BasiliskKernel>();

string response = await kernel.ChatAsync("Hello, Dungeon Master!");
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
    collection.AddScoped<ClassesPlugin>();
    collection.AddScoped<RacesPlugin>();
    collection.AddScoped<QuestionAnsweringPlugin>();
    
    collection.AddScoped<BasiliskKernel>(s => new(s, config.AzureOpenAiDeploymentName,
        config.AzureOpenAiEndpoint,
        config.AzureOpenAiKey));

    return collection.BuildServiceProvider();
}