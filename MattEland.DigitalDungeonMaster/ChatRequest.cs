namespace MattEland.DigitalDungeonMaster;

public class ChatRequest
{
    public required string Message { get; set; }
    public bool ClearFirst { get; set; } = true;
}