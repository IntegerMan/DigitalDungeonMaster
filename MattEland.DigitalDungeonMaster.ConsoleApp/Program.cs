using MattEland.DigitalDungeonMaster;
using MattEland.DigitalDungeonMaster.ConsoleApp;
using MattEland.DigitalDungeonMaster.ConsoleApp.Menus;
using MattEland.DigitalDungeonMaster.ServiceDefaults;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ServiceDiscovery;

// Create the host
HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Services.AddHostedService<DigitalDungeonMasterWorker>();
builder.AddServiceDefaults();

// Web communications
builder.Services.ConfigureHttpClientDefaults(http => http.AddServiceDiscovery());
builder.Services.Configure<ServiceDiscoveryOptions>(o => o.AllowAllSchemes = true);
builder.Services.AddScoped<ApiClient>();

// Request context
builder.Services.AddScoped<RequestContextService>();

// Front-end menus
builder.Services.AddScoped<LoadGameMenu>();
builder.Services.AddScoped<NewGameMenu>();
builder.Services.AddScoped<LoginMenu>();
builder.Services.AddScoped<MainMenu>();
builder.Services.AddScoped<AdventureRunner>();

// Configuration options
builder.Services.Configure<AgentConfig>(c => builder.Configuration.Bind("Agents:DungeonMaster", c));
builder.Services.Configure<UserSavedInfo>(c => builder.Configuration.Bind("UserInfo", c));
builder.Services.Configure<ServerSettings>(c => builder.Configuration.Bind("Server", c));

// Run the application
IHost app = builder.Build();
await app.RunAsync();
/*
    // Configure logging
    services.AddScoped<ILoggerFactory, LoggerFactory>();
    services.AddLogging(builder =>
    {
        builder.ClearProviders();
        builder.SetMinimumLevel(LogLevel.Trace);
        builder.AddNLog("NLog.config");
    });
*/