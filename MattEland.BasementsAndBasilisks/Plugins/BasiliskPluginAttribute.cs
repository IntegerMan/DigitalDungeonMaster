namespace MattEland.BasementsAndBasilisks.Plugins;

[AttributeUsage(AttributeTargets.Class)]
public class BasiliskPluginAttribute : Attribute
{
    public required string PluginName { get; init; }
}