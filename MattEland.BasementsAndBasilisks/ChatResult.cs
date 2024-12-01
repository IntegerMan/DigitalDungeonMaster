using MattEland.BasementsAndBasilisks.Blocks;

namespace MattEland.BasementsAndBasilisks;

public class ChatResult
{
    public required string Message { get; init; }
    public required IEnumerable<ChatBlockBase> Blocks { get; init; }
}