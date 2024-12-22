using Avalonia;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Avalonia.Logging;
using Lemon.Hosting.AvaloniauiDesktop;
using MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;
using MattEland.DigitalDungeonMaster.ClientShared;
using Microsoft.Extensions.Hosting;
using MattEland.DigitalDungeonMaster.ServiceDefaults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp;

sealed class Program
{
    [STAThread]
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [RequiresDynamicCode("Calls Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder()")]
    public static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        
        // Support Aspire service defaults
        builder.AddServiceDefaults();

        // config IConfiguration
        builder.Configuration
            .AddCommandLine(args)
            .AddEnvironmentVariables()
            .AddInMemoryCollection();

        // Add logging
        builder.Services.AddLogging(logBuilder =>
        {
            logBuilder.AddConsole();
            logBuilder.AddDebug();
            logBuilder.AddOpenTelemetry();
            logBuilder.SetMinimumLevel(LogLevel.Trace);
        });
        
        // Web communications
        builder.Services.ConfigureHttpClientDefaults(http => http.AddServiceDiscovery());
        builder.Services.Configure<ServiceDiscoveryOptions>(o => o.AllowAllSchemes = true);
        builder.Services.AddScoped<ApiClient>();
        builder.Services.Configure<ApiClientOptions>(o =>
        {
            // If we're in Aspire, use the Aspire base URL. Service Discovery will substitute the endpoint of the local service here.
            if (args.Contains("--aspire", StringComparer.OrdinalIgnoreCase))
            {
                o.BaseUrl = "https+http://WebAPI";
            }
        });
        
        // Find every type in this assembly that ends in ViewModel, Page, or Service and register it for DI
        foreach (Type type in typeof(Program).Assembly.GetTypes())
        {
            if (type.Name.EndsWith("ViewModel") || type.Name.EndsWith("Page") || type.Name.EndsWith("Service"))
            {
                builder.Services.AddScoped(type);
            }
        }

        RunAppDefault(builder, args);
    }

    private static AppBuilder ConfigAvaloniaAppBuilder(AppBuilder appBuilder)
    {
        return appBuilder
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace(LogEventLevel.Verbose);
    }

    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void RunAppDefault(HostApplicationBuilder hostBuilder, string[] args)
    {
        hostBuilder.Services.AddAvaloniauiDesktopApplication<App>(ConfigAvaloniaAppBuilder);
        
        // build host
        IHost appHost = hostBuilder.Build();
        
        // run app
        appHost.RunAvaloniauiApplication(args);
    }
}