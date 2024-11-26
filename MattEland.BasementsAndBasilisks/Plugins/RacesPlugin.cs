using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
public class RacesPlugin
{
    [KernelFunction("GetRaces")]
    [Description("Gets a list of available races in the game.")]
    [return: Description("A list of races characters can play as")]
    public IEnumerable<string> GetRaces()
    {
        yield return "Aasimar";
        yield return "Dragonborn";
        yield return "Dwarf";
        yield return "Elf";
        yield return "Goliath";
        yield return "Halfling";
        yield return "Human";
        yield return "Orc";
        yield return "Tiefling";
        yield return "Half-Elf";
    }
}