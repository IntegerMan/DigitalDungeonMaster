namespace MattEland.DigitalDungeonMaster.Shared;

public interface IChatRequest
{
    IEnumerable<ChatMessage>? History { get; set; }
    ChatMessage Message { get; set; } 
    string? RecipientName { get; set; }
    Guid? Id { get; set; }
    string User { get; set; }
}