namespace MattEland.DigitalDungeonMaster.Shared;

public interface IChatRequest
{
    IEnumerable<ChatMessage>? History { get; set; }
    string Message { get; set; } // TODO: This should probably be a ChatMessage
    string? RecipientName { get; set; }
    Guid? Id { get; set; }
    string User { get; set; }
}