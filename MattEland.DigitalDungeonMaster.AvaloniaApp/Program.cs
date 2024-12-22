using Avalonia;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
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
        // TODO: Somewhere the ApiClient should be configured with the base URL when Aspire is not used to launch the app!
        
        // Set up View Models
        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddTransient<LoginViewModel>();

        RunAppDefault(builder, args);
    }

    private static AppBuilder ConfigAvaloniaAppBuilder(AppBuilder appBuilder)
    {
        return appBuilder
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
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