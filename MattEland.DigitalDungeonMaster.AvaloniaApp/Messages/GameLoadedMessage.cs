using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;

public class GameLoadedMessage
{
    public GameLoadedMessage(AdventureInfo adventure)
    {
        Adventure = adventure;
    }

    public AdventureInfo Adventure { get; }
    
    public override string ToString() => $"Game {Adventure.Name} has been loaded";
}