using System.Text;

namespace MattEland.DigitalDungeonMaster.Shared;

public class ChatResult
{
    public required IEnumerable<ChatMessage> Replies { get; init; }
    public required Guid Id { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new("Conversation " + Id);
        sb.AppendLine();
        foreach (var reply in Replies)
        {
            sb.AppendLine(reply.ToString());
        }

        return sb.ToString();
    }
}