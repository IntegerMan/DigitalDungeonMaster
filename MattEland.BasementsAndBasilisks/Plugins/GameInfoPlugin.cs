namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[BasiliskPlugin(PluginName = "GameInformation")]
public class GameInfoPlugin
{
    [KernelFunction("GetSettingAndTone")]
    [Description("Gets information about the game world and setting")]
    [return: Description("Advice on detailing the setting of the game world.")]
    public string GetSettingAndTone()
    {
        // TODO: This is probably better pulled from a local file, database, or blob storage
        return """
               The game is set in a fantasy setting that is a mixture of technological progress, magic, ancient ruins of dead races,
               wilderness, and alien lands of truly foreign nature (levitating amorphous blobs that are sentient, for example).
               Avoid typical fantasy tropes like elves, dwarves, and orcs. Instead, focus on the unique and the strange.

               The game is focused on exploration, discovery, and problem-solving. Combat is a part of the game, but it is not the primary focus.
               The player wants agency over their own actions and wants to see a setting develop over time.
               """;
    }

    [KernelFunction("GetGameMasterStyle")]
    [Description("Gets guidance on how the game master should interact with players")]
    [return: Description("Advice on the best ways of creating a compelling experience.")]
    public string GetGameMasterStyle()
    {
        // TODO: This is probably better pulled from a local file, database, or blob storage
        return """
               Your style as a Game Master should be focused on player agency, exploration, and discovery. Make the world interesting
               and give players opportunities to make interesting choices, use their skills, and interact with the world in meaningful ways.
               """;
    }
}