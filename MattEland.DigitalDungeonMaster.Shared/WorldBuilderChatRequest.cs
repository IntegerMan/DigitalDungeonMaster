namespace MattEland.DigitalDungeonMaster.Shared;

public class WorldBuilderChatRequest : IChatRequest
{
    public IEnumerable<ChatMessage>? History { get; set; }
    public required ChatMessage Message { get; set; } 
    public string? RecipientName { get; set; }
    public Guid? Id { get; set; }
    public required string User { get; set; }
    
    public required NewGameSettingInfo Data { get; init; }
}