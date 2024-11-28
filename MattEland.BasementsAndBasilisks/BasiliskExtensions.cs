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
    }

    public static void RegisterBasiliskPlugins(this ServiceCollection services)
    {
        // Find all Types that have the BasiliskPluginAttribute and register them as services
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.GetCustomAttribute<BasiliskPluginAttribute>() != null)
            {
                // TODO: Instead of this we could also provide a PlugInCollection
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