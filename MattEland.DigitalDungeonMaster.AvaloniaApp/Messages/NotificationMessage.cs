using Avalonia.Controls.Notifications;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;

public class NotificationMessage
{
    public NotificationMessage(string title, string message, NotificationType notificationType = NotificationType.Information)
    {
        Title = title;
        Message = message;
        NotificationType = notificationType;
    }

    public string Title { get; }
    public string Message { get; }
    public NotificationType NotificationType { get; }
    
    public override string ToString() => $"{NotificationType}: {Title} - {Message}";
}