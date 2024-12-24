using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MattEland.DigitalDungeonMaster;

public abstract class PluginBase
{
    private readonly ActivitySource _activitySource;
    
    protected PluginBase(ILogger logger)
    {
        Logger = logger;
        _activitySource = new ActivitySource(GetType().Assembly.FullName ?? GetType().Assembly.GetName().Name ?? GetType().FullName ?? "Unknown");
    }
    
    protected ILogger Logger { get; }
    
    protected Activity? LogActivity(string? message, [CallerMemberName] string? memberName = null)
    {
        string pluginName = GetType().Name;
        Logger.LogDebug("{PluginName}-{MemberName} called {Message}", pluginName, memberName, message);
        
        return _activitySource.StartActivity($"{pluginName}-{memberName} called {message}");
    }
}