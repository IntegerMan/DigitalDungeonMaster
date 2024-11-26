using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
public class ClassesPlugin
{
    [KernelFunction("GetClasses")]
    [Description("Gets a list of available classes in the game.")]
    [return: Description("A list of classes characters can play as")]
    public IEnumerable<ClassModel> GetClasses()
    {
        Console.WriteLine("GetClasses Called");
        return new List<ClassModel> {
            new() { Name = "Artificer", Description = "A class that uses magic-infused objects to cast spells and create gadgets"},
            new() { Name = "Barbarian", Description = "A combat class centered on bouts of rage"},
            new() { Name = "Bard", Description = "A support class that uses music and magic to inspire buff allies and debuff foes"},
            new() { Name = "Cleric", Description = "A front-line support class that uses divine magic to heal and protect allies"},
            new() { Name = "Druid", Description = "A nature-based class that can shapeshift and cast spells"},
            new() { Name = "Fighter", Description = "A versatile front line tactician that can specialize in a variety of weapons and tactics"},
            new() { Name = "Monk", Description = "A martial artist that can perform incredible feats"},
            new() { Name = "Paladin", Description = "A holy warrior that can heal, protect, and smite foes"},
            new() { Name = "Ranger", Description = "A wilderness warrior that can track, hunt, and survive in the wild"},
            new() { Name = "Rogue", Description = "A stealthy and cunning class that can sneak, pick locks, and deal massive damage"},
            new() { Name = "Sorcerer", Description = "A spellcaster that can cast spells innately and modify magic"},
            new() { Name = "Warlock", Description = "A spellcaster that has limited spell slots but can cast spells at will"},
            new() { Name = "Wizard", Description = "A spellcaster that learns and prepares spells from a spellbook with a wide variety of spells"}
        };
    }

    public class ClassModel {
        public string Name { get; set; }
        
        public string Description { get; set; }
    }
}