using Azure;
using Azure.AI.OpenAI;
using MattEland.DigitalDungeonMaster;
using MattEland.DigitalDungeonMaster.ConsoleApp;
using MattEland.DigitalDungeonMaster.ConsoleApp.Menus;
using MattEland.DigitalDungeonMaster.Models;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel.TextToImage;
using NLog.Extensions.Logging;

#pragma warning disable SKEXP0001

#pragma warning disable SKEXP0010 // Text to image service

ILogger<Program>? logger = null;

try
{
    // Using UTF8 allows more capabilities for Spectre.Console.
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;

    // Display the header
    DisplayHelpers.RenderHeader();

    IServiceProvider serviceProvider = RegisterServices();
    logger = serviceProvider.GetRequiredService<ILogger<Program>>();

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
            (keepGoing, bool isNewAdventure) = await mainMenu.RunAsync();

            if (keepGoing && context.CurrentAdventure is not null)
            {
                keepGoing = await adventureRunner.RunAsync(isNewAdventure);
            }
        }
    }
    while (keepGoing);

    DisplayHelpers.SayDungeonMasterLine("Goodbye, Adventurer!");
}
catch (Exception ex)
{
    logger?.LogCritical(ex, "An unhandled exception of type {Type} occurred in the main loop: {Message}",
        ex.GetType().FullName, ex.Message);
    
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
}

IServiceProvider RegisterServices()
{
    ServiceCollection services = new();
    
    // Configure logging
    services.AddScoped<ILoggerFactory, LoggerFactory>();
    services.AddLogging(builder =>
    {
        builder.ClearProviders();
        builder.SetMinimumLevel(LogLevel.Trace);
        builder.AddNLog("NLog.config");
    });
    
    // Front-end menus
    services.AddScoped<LoadGameMenu>();
    services.AddScoped<NewGameMenu>();
    services.AddScoped<LoginMenu>();
    services.AddScoped<MainMenu>();
    services.AddScoped<AdventureRunner>();
    
    // Configure the kernel
    // Read the configuration from appsettings.json and user secrets
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddUserSecrets<Program>()
        .Build();

    // Configuration options
    services.Configure<AgentConfig>(c => configuration.Bind("Agents:DungeonMaster", c));
    services.Configure<AzureResourceConfig>(c => configuration.Bind("AzureResources", c));

    // Set up AI resources
    services.AddScoped<AzureOpenAIClient>(s =>
    {
        IOptionsSnapshot<AzureResourceConfig> config = s.GetRequiredService<IOptionsSnapshot<AzureResourceConfig>>();
        Uri endpoint = new Uri(config.Value.AzureOpenAiEndpoint);
        AzureKeyCredential credential = new(config.Value.AzureOpenAiKey);
        
        return new(endpoint, credential);
    });        
    services.AddScoped<AzureOpenAIChatCompletionService>(s =>
    {
        AzureOpenAIClient client = s.GetRequiredService<AzureOpenAIClient>();
        IOptionsSnapshot<AzureResourceConfig> config = s.GetRequiredService<IOptionsSnapshot<AzureResourceConfig>>();
        AzureOpenAIChatCompletionService chat = new(
            config.Value.AzureOpenAiChatDeploymentName,
            client);
        
        return chat;
    });        
    services.AddScoped<IChatCompletionService>(s =>
    {
        AzureOpenAIChatCompletionService chat = s.GetRequiredService<AzureOpenAIChatCompletionService>();
        return chat;
    });    
    services.AddScoped<ITextGenerationService>(s =>
    {
        AzureOpenAIChatCompletionService chat = s.GetRequiredService<AzureOpenAIChatCompletionService>();
        return chat;
    });    
    services.AddScoped<ITextToImageService>(s =>
    {
        AzureOpenAIClient client = s.GetRequiredService<AzureOpenAIClient>();
        IOptionsSnapshot<AzureResourceConfig> config = s.GetRequiredService<IOptionsSnapshot<AzureResourceConfig>>();
        return new AzureOpenAITextToImageService(config.Value.AzureOpenAiImageDeploymentName, client, null);
    });
    
    services.AddScoped<MainKernel>();
    services.RegisterGameServices();
    services.RegisterGamePlugins();

    return services.BuildServiceProvider();
}

