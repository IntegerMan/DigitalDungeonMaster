using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
//[BasiliskPlugin(PluginName = "Skills")]
public class SkillsPlugin
{
    private readonly RequestContextService _context;

    public SkillsPlugin(RequestContextService context)
    {
        _context = context;
    }
    
    [KernelFunction("GetSkills")]
    [Description("Gets a list of skills in the game and their uses.")]
    [return: Description("A list of skills and their uses")]
    public IEnumerable<SkillSummary> GetSkills()
    {
        _context.LogPluginCall();
        
        // TODO: This would be better from an external data source
        return new List<SkillSummary> {
            new() { Name = "Acrobatics", Attribute = "Dexterity", Description = "The ability to perform feats of agility and balance" },
            new() { Name = "Animal Handling", Attribute = "Wisdom", Description = "Managing and training animals and communicating with them" },
            new() { Name = "Arcana", Attribute = "Intelligence", Description = "Understanding magical constructs, writing, and apparatus" },
            new() { Name = "Athletics", Attribute = "Strength", Description = "Physical feats of strength and endurance" },
            new() { Name = "Deception", Attribute = "Charisma", Description = "The ability to lie, cheat, and hide your intent" },
            new() { Name = "History", Attribute = "Intelligence", Description = "The ability to recall and relate historical lore you've been exposed to" },
            new() { Name = "Insight", Attribute = "Wisdom", Description = "Understanding what motivates another person and detecting hidden intent and emotion" },
            new() { Name = "Intimidation", Attribute = "Charisma", Description = "The ability to influence others through threats, fear, and coercion" },
            new() { Name = "Investigation", Attribute = "Intelligence", Description = "Understanding the world by examining details, interactions, and trace clues" },
            new() { Name = "Medicine", Attribute = "Wisdom", Description = "The ability to heal wounds, diagnose illness, and provide medical care" },
            new() { Name = "Nature", Attribute = "Intelligence", Description = "The ability to recall and infer lore about plants, animals, weather, and natural cycles" },
            new() { Name = "Perception", Attribute = "Wisdom", Description = "The ability to spot, hear, or otherwise detect the presence of something" },
            new() { Name = "Performance", Attribute = "Charisma", Description = "The ability to entertain, inspire, and influence others through music, dance, and oratory" },
            new() { Name = "Persuasion", Attribute = "Charisma", Description = "The ability to influence others through tact, social graces, and diplomacy" },
            new() { Name = "Religion", Attribute = "Intelligence", Description = "The ability to recall or infer lore about deities, rites, prayers, and religious hierarchies" },
            new() { Name = "Sleight of Hand", Attribute = "Dexterity", Description = "The ability to perform tricks, pick pockets, hide objects, and manipulate mechanical devices" },
            new() { Name = "Stealth", Attribute = "Dexterity", Description = "The ability to move silently, hide, and avoid detection" },
            new() { Name = "Survival", Attribute = "Wisdom", Description = "The ability to follow tracks, hunt wild game, navigate through the wilderness, and identify signs of civilization" }
        };
    }

    public class SkillSummary {
        public required string Name { get; set; }
        public required string Attribute { get; set; }
        
        public required string Description { get; set; }
    }
}