using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MattEland.BasementsAndBasilisks.Plugins;

public static class PluginExtensions
{
    public static void RegisterBasiliskPlugins(this ServiceCollection services)
    {
        // Find all Types that have the BasiliskPluginAttribute and register them as services
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.GetCustomAttribute<BasiliskPluginAttribute>() != null)
            {
                services.AddScoped(type);
            }
        }
    }

    public static void RegisterBasiliskPlugins(this Kernel kernel, IServiceProvider services) 
        => kernel.Plugins.RegisterBasiliskPlugins(services);

    public static void RegisterBasiliskPlugins(this KernelPluginCollection plugins, IServiceProvider services)
    {
        // Find all Types that have the BasiliskPluginAttribute, instantiate them using services, and register them as plugins
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            BasiliskPluginAttribute? attribute = type.GetCustomAttribute<BasiliskPluginAttribute>();
            if (attribute != null)
            {
                plugins.AddFromObject(services.GetRequiredService(type), attribute.PluginName, services);
            }
        }
    }
}