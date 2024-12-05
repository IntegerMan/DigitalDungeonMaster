using System.Reflection;
using MattEland.BasementsAndBasilisks.Plugins;
using MattEland.BasementsAndBasilisks.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MattEland.BasementsAndBasilisks;

public static class PluginExtensions
{
    public static void RegisterBasiliskServices(this ServiceCollection services)
    {
        services.AddScoped<RandomService>();
        services.AddScoped<RequestContextService>();
        services.AddScoped<StorageDataService>();
    }

    public static void RegisterBasiliskPlugins(this ServiceCollection services)
    {
        foreach (Type type in Assembly.GetExecutingAssembly()
                     .GetTypes()
                     .Where(t => t.IsAssignableTo(typeof(BasiliskPlugin)) && !t.IsAbstract))
        {
            services.AddScoped(type);
        }
    }

    public static void RegisterBasiliskPlugins(this Kernel kernel, IServiceProvider services)
    {
        foreach (Type type in Assembly.GetExecutingAssembly()
                     .GetTypes()
                     .Where(t => t.IsAssignableTo(typeof(BasiliskPlugin)) && !t.IsAbstract))
        {
            BasiliskPlugin plugin = (BasiliskPlugin)services.GetRequiredService(type);
            plugin.Kernel = kernel;
            
            kernel.Plugins.AddFromObject(plugin, type.Name, services);
        }
    }
}