namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;

public class LoggedOutMessage
{
    public LoggedOutMessage(string username)
    {
        Username = username;
    }

    public string Username { get; }
    
    public override string ToString() => $"User {Username} has logged out";
}