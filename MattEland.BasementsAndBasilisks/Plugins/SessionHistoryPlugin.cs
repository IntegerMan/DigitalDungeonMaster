namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "SessionHistory")]
public class SessionHistoryPlugin
{
    [KernelFunction("GetLastSessionRecap")]
    [Description("Gets a short recap of our last adventuring session")]
    [return: Description("A short recap of the last adventuring session")]
    public string GetLastSessionRecap()
    {
        // TODO: This is probably better pulled from a local file, database, or blob storage
        return """
               **Full Story Recap**:  
               Norrick, a human artificer, found himself unexpectedly transported to an unfamiliar wilderness due to a mishap involving a teleportation crystal during one of his experiments. Stranded in this strange new world, he discovered a damaged fragment of a teleportation crystal in the remnants of his accident, sparking hope for a way back home. Determined to uncover the mysteries surrounding his situation, Norrick set out toward a decrepit tower in the distance, believing it to hold crucial clues. As he navigated the dense forest, he encountered various landmarks and strange markings on the trees that hinted at a past connected to nature magic. Upon reaching the tower, he explored its cavernous interiors, uncovering remnants of magical experimentation and relics of forgotten knowledge. However, his investigation was interrupted by the sudden appearance of a Scraggle, a small alien creature that challenged him, presenting a fresh test of his skills and resolve in this alarming new reality.
               
               **Journey to the Tower Recap**:  
               Norrick's trek through the dense wilderness led him southward to a decaying tower that loomed tall amidst the trees. The path was rugged, littered with rocks and roots, requiring careful navigation. While alert for danger, he encountered a mix of tranquil wildlife and strange tree markings suggesting a past of potential druidic influence. As Norrick approached the tower, he felt a mixture of anticipation and foreboding, aware of the mysteries that lay ahead.
               
               **Tower Description Recap**:  
               The tower stood at approximately 60 feet tall, its crumbling stone exterior revealing neglect and the passage of time. Inside, an expansive chamber housed the remnants of what appeared to be a former magical laboratory, with scattered scrolls and ancient diagrams hinting at a history of arcane study. Dust motes danced in the shafts of light filtering through the broken windows, while faintly glowing, archaic symbols etched into the stone floor suggested a lingering, dormant power. The atmosphere within was heavy with the echoes of past knowledge, making it clear that this tower was once a place of significance.
               
               **Encounter with the Scraggle Recap**:  
               Within the tower's shadows, Norrick was confronted by a Scraggle—a small, goblin-like alien creature that embodied both curiosity and aggression. With green, bark-like skin and oversized, bulbous eyes, the scrappy little alien brandished a crude dagger. Its chaotic appearance belied a cunning nature, ready to defend its territory against intruders. The Scraggle’s unexpected challenge marked the beginning of Norrick's first test of combat in this strange new world, hinting at further surprises that lay in wait.  
               """;
    }
}