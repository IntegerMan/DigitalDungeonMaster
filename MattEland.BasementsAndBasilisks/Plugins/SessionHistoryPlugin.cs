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
                               Norrick, a human artificer, found himself in a strange wilderness after a teleportation mishap. Stranded and disoriented, he discovered remnants of a failed experiment—a damaged teleportation crystal—leading him to believe that the nearby decrepit tower might hold the clues necessary for his return home. As he ventured through the dense forest toward the tower, he encountered various landmarks, including strange tree markings that hinted at a history tied to nature magic.

                               Upon reaching the tower, Norrick explored its crumbling interiors, uncovering remnants of magical experimentation in an expansive chamber. His investigation was interrupted by an unexpected encounter with a small, cunning creature known as a Scraggle, which challenged him in combat. Norrick managed to fend off the creature, securing a scraggle dagger it dropped during its escape, and successfully fortified the door to the tower using magic and physical means, creating a spooky noise to deter potential intruders.

                               Inside the tower, he examined the second floor, finding a writing desk, a broken vase of alchemical nature, and arcane symbols etched into the stone floor, hinting at a warding circle that once protected the tower. He salvaged a weathered journal filled with alien script, along with a map depicting the surrounding landscape that marked important locations: the nearby wilderness, an arrival clearing, a Circle of Stones to the northeast, and creature dens to the south-southwest near a brook. The markings on the trees in the forest suggested an ancient connection to nature magic.

                               As Norrick secured his temporary shelter, contemplating his next moves, he held onto the knowledge gleaned from the tower and the promise of new adventures that awaited him beyond its walls.
                               """;
        
        _context.AddBlock(new TextResourceBlock("Session Recap", recap));
        return recap;
    }
}