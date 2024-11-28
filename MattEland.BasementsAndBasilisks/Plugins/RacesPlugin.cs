using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "Races")]
public class RacesPlugin
{
    private readonly RequestContextService _context;

    public RacesPlugin(RequestContextService context)
    {
        _context = context;
    }
    
    [KernelFunction("GetRaces")]
    [Description("Gets a list of available races in the game.")]
    [return: Description("A list of races characters can play as")]
    public IEnumerable<string> GetRaces()
    {
        _context.LogPluginCall();
        
        // TODO: These should come from a JSON file or database table or something external
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