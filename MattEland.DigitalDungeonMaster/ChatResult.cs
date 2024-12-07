using MattEland.DigitalDungeonMaster.Blocks;

namespace MattEland.DigitalDungeonMaster;

public class ChatResult
{
    public required string Message { get; init; }
    public required IEnumerable<ChatBlockBase> Blocks { get; init; }
}