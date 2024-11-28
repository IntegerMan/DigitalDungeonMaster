using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "SessionHistory")]
public class SessionHistoryPlugin
{
    private readonly RequestContextService _context;

    public SessionHistoryPlugin(RequestContextService context)
    {
        _context = context;
    }
    
    [KernelFunction("GetLastSessionRecap")]
    [Description("Gets a short recap of our last adventuring session")]
    [return: Description("A short recap of the last adventuring session")]
    public string GetLastSessionRecap()
    {
        _context.LogPluginCall();
        
        // TODO: This is probably better pulled from a local file, database, or blob storage
        string recap = """
                               In the strange wilderness of an alien world, Norrick, a human artificer, found himself stranded after a teleportation mishap. Navigating through the dense forest, he stumbled upon a decrepit tower that hinted at magical experimentation. Within its crumbling walls, he battled a cunning Scraggle and fortified the door using his innate magic. Exploring the tower further, Norrick uncovered a weathered journal filled with alien script and a map marking locations of interest.
                               
                               Realizing he must meet his basic needs, he sought food and water. After collecting herbs with healing properties and encountering peculiar feathered creatures resembling a mix of hares and birds, he successfully hunted one. In a resourceful display, Norrick cooked the hare over a campfire and dried the herbs for future use. When night approached, he constructed a makeshift sleeping area from the tower's remnants, using wood and feathers to create a comfortable place to rest. With dawn breaking, Norrick prepared to continue his adventure, fully aware of the challenges and mysteries still awaiting him in this enigmatic world.
                               
                               This summary captures Norrick's journey and preparations for his next session, highlighting the key events and discoveries that have shaped his experience thus far.
                               """;
        
        _context.AddBlock(new TextResourceBlock("Session Recap", recap));
        return recap;
    }
}