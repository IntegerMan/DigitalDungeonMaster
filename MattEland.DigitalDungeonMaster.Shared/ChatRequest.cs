namespace MattEland.DigitalDungeonMaster.Shared;

public class ChatRequest
{
    public IEnumerable<ChatMessage>? History { get; set; }
    public required string Message { get; set; } // TODO: This should probably be a ChatMessage
    public string? RecipientName { get; set; }
    public Guid? Id { get; set; }
    public required string User { get; set; }
}