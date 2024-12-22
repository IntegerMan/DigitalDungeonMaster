namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;

public class LoggedInMessage
{
    public LoggedInMessage(string username)
    {
        Username = username;
    }

    public string Username { get; }
}