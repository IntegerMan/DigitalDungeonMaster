using MattEland.BasementsAndBasilisks.Models;
using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "Players")]
public class PlayersPlugin
{
    private readonly RequestContextService _context;

    public PlayersPlugin(RequestContextService context)
    {
        _context = context;
    }
    
    // TODO: This Lazy pattern isn't async and can't work with instance data, which would make this a good candidate for a service
    private Dictionary<string, PlayerDetails>? _players;

    private void EnsurePlayersLoaded()
    {
        if (_players != null) return;
        
        // TODO: This really should come from a database or some other persistent storage - maybe even D&D Beyond
        _players = new Dictionary<string, PlayerDetails>(StringComparer.OrdinalIgnoreCase)
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
                    
                    Stats = new EntityAttributes
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
        _context.LogPluginCall();
        EnsurePlayersLoaded();
        
        return _players!.Keys;
    }

    [KernelFunction("GetPlayerDetails")]
    public PlayerDetails? GetPlayerDetails(string playerName)
    {
        _context.LogPluginCall(metadata: playerName);
        EnsurePlayersLoaded();
        
        return _players!.GetValueOrDefault(playerName);
    }
}