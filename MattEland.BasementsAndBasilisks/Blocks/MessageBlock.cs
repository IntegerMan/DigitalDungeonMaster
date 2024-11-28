namespace MattEland.BasementsAndBasilisks.Blocks;

public class MessageBlock : ChatBlockBase
{
    public required string Message { get; init; }
    public bool IsUserMessage { get; init; } = false;
}