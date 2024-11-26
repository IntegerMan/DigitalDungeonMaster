namespace MattEland.BasementsAndBasilisks.Plugins;

/// <summary>
/// Represents a plugin for the Basements and Basilisks game.
/// Classes with this attribute will be automatically registered into the service collection for dependency injection
/// and automatically added to Semantic Kernel.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BasiliskPluginAttribute : Attribute
{
    public required string PluginName { get; init; }
}