namespace MattEland.DigitalDungeonMaster.Shared;

public class ChatRequest
{
    public required string Message { get; set; }
    public string? RecipientName { get; set; }
    public Guid? Id { get; set; }
}