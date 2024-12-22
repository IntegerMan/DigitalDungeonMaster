using Avalonia;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Lemon.Hosting.AvaloniauiDesktop;
using MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;
using Microsoft.Extensions.Hosting;
using MattEland.DigitalDungeonMaster.ServiceDefaults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        HostApplicationBuilder hostBuilder = Host.CreateApplicationBuilder();
        
        // Support Aspire service defaults
        hostBuilder.AddServiceDefaults();

        // config IConfiguration
        hostBuilder.Configuration
            .AddCommandLine(args)
            .AddEnvironmentVariables()
            .AddInMemoryCollection();

        // Add logging
        hostBuilder.Services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.AddOpenTelemetry();
            builder.SetMinimumLevel(LogLevel.Trace);
        });
        
        // Set up View Models
        hostBuilder.Services.AddSingleton<MainWindowViewModel>();
        hostBuilder.Services.AddTransient<LoginViewModel>();

        RunAppDefault(hostBuilder, args);
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