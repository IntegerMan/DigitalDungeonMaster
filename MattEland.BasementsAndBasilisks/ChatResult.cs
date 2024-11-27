namespace MattEland.BasementsAndBasilisks;

public class ChatResult
{
    public required string Message { get; init; }
    public required IEnumerable<string> FunctionsCalled { get; init; }
}