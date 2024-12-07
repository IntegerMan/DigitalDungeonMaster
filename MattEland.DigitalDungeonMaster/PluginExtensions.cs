using System.Reflection;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MattEland.DigitalDungeonMaster;

public static class PluginExtensions
{
    public static void RegisterGameServices(this ServiceCollection services)
    {
        services.AddScoped<RandomService>();
        services.AddScoped<RequestContextService>();
        services.AddScoped<RulesetService>();
        services.AddScoped<StorageDataService>();
        services.AddScoped<LocationGenerationService>();
        services.AddScoped<UserService>();
    }

    public static void RegisterGamePlugins(this ServiceCollection services)
    {
        foreach (Type type in Assembly.GetExecutingAssembly()
                     .GetTypes()
                     .Where(t => t.IsAssignableTo(typeof(GamePlugin)) && !t.IsAbstract))
        {
            services.AddScoped(type);
        }
    }

    public static void RegisterGamePlugins(this Kernel kernel, IServiceProvider services)
    {
        foreach (Type type in Assembly.GetExecutingAssembly()
                     .GetTypes()
                     .Where(t => t.IsAssignableTo(typeof(GamePlugin)) && !t.IsAbstract))
        {
            GamePlugin plugin = (GamePlugin)services.GetRequiredService(type);
            plugin.Kernel = kernel;
            
            kernel.Plugins.AddFromObject(plugin, type.Name, services);
        }
    }
}