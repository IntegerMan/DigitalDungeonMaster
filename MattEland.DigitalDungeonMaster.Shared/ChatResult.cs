namespace MattEland.DigitalDungeonMaster.Shared;

public class ChatResult
{
    public required IEnumerable<ChatMessage> Replies { get; init; }
}