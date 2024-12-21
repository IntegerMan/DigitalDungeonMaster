namespace MattEland.DigitalDungeonMaster.Shared;

public interface IChatResult
{
    IEnumerable<ChatMessage>? Replies { get; init; }
    Guid Id { get; set; }
    bool IsError { get; set; }
}