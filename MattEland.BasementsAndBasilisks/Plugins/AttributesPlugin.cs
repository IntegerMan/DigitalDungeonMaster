using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
//[BasiliskPlugin(PluginName = "Attributes")]
public class AttributesPlugin
{
    private readonly RequestContextService _context;

    public AttributesPlugin(RequestContextService context)
    {
        _context = context;
    }
    
    [KernelFunction("GetAttributes")]
    [Description("Gets a list of attributes in the game and their uses.")]
    [return: Description("A list of attributes and their uses")]
    public IEnumerable<AttributeSummary> GetAttributes()
    {
        _context.LogPluginCall();
        
        return new List<AttributeSummary>
        {
            new() { Name = "Strength", Description = "The ability to exert physical force and perform feats of strength" },
            new() { Name = "Dexterity", Description = "The ability to perform feats of agility, balance, and precision" },
            new() { Name = "Constitution", Description = "The ability to endure physical hardship, resist disease, and recover from injury" },
            new() { Name = "Intelligence", Description = "The ability to reason, recall information, and solve problems" },
            new() { Name = "Wisdom", Description = "The ability to perceive the world around you, understand it, and make good decisions" },
            new() { Name = "Charisma", Description = "The ability to influence others, lead, and inspire" }
        };
    }

    public class AttributeSummary {
        public required string Name { get; set; }
        
        public required string Description { get; set; }
    }
}