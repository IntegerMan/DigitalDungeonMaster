using System.Text;

namespace MattEland.DigitalDungeonMaster.Shared;

public record ChatMessage
{
    public string? Message { get; init; }
    public string? ImageUrl { get; init; }
    public required string Author { get; init; }

    public override string ToString()
    {
        StringBuilder sb = new($"{Author}: ");
        if (Message is not null)
        {
            sb.Append(Message);
        }
        if (ImageUrl is not null)
        {
            sb.Append($" ({ImageUrl})");
        }

        return sb.ToString();
    }
}