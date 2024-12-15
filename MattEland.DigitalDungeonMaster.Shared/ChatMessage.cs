namespace MattEland.DigitalDungeonMaster.Shared;

public record ChatMessage
{
    public string? Message { get; init; }
    public string? ImageUrl { get; init; }
    public required string Author { get; init; }
}