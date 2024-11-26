using MattEland.BasementsAndBasilisks.Models;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "Players")]
public class PlayersPlugin
{
    // TODO: This Lazy pattern isn't async and can't work with instance data, which would make this a good candidate for a service
    private Lazy<Dictionary<string, PlayerDetails>> _players = new(LoadPlayers);

    private static Dictionary<string, PlayerDetails> LoadPlayers()
    {
        // TODO: This really should come from a database or some other persistent storage - maybe even D&D Beyond
        return new Dictionary<string, PlayerDetails>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Norrick",
                new PlayerDetails
                {
                    Name = "Norrick",
                    Nickname = "Norrick the Lost",
                    Species = "Human",
                    Gender = "Male",
                    Description = "An artificer who teleported himself into an unknown world on accident",
                    Goals = "Long-term: Find a way home by fixing the magical crystal. Short-term: Orient himself and meet survival needs",
                    Motivations = "Wants to gain power and capability, wants to be capable, wants safety and security",
                    Alignment = "Neutral",
                    
                    // TODO: This isn't nuanced enough for multi-classing and should evolve to support that
                    Level = 1,
                    PlayerClass = "Artificer",
                    
                    
                    HitPoints = new HitPoints(10, 10, 0),
                    
                    Stats = new StatsBlock
                    {
                        Strength = 10,
                        Dexterity = 12,
                        Constitution = 14,
                        Intelligence = 17,
                        Wisdom = 14,
                        Charisma = 10
                    }
                }
            },
        };
    }

    [KernelFunction("GetPlayerCharacters")]
    [Description("Gets a list of player characters in the play session")]
    [return: Description("A list of player characters in the play session")]
    public IEnumerable<string> GetPlayerCharacters()
    {
        return _players.Value.Keys;
    }

    [KernelFunction("GetPlayerDetails")]
    public PlayerDetails? GetPlayerDetails(string playerName)
    {
        if (_players.Value.TryGetValue(playerName, out var details))
        {
            return details;
        }

        return null; // TODO: Is Semantic Kernel okay with null returns?
    }
}