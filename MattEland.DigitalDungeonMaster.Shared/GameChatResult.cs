namespace MattEland.DigitalDungeonMaster.Shared;

public class GameChatResult : IChatResult
{
    public required IEnumerable<ChatMessage> Replies { get; init; }
    public Guid Id { get; set; }
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<string> AvailableAgents { get; set; } = [];
}