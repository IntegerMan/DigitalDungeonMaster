using Avalonia.Controls.Notifications;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Services;

public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly EventsService _events;

    public NotificationService(ILogger<NotificationService> logger, EventsService events)
    {
        _logger = logger;
        _events = events;
    }
    
    public void ShowSuccess(string title, string message)
    {
        ShowNotification(new NotificationMessage(title, message, NotificationType.Success));
    }    
    
    public void ShowWarning(string title, string message)
    {
        ShowNotification(new NotificationMessage(title, message, NotificationType.Warning));
    }    
    
    public void ShowInfo(string title, string message)
    {
        ShowNotification(new NotificationMessage(title, message, NotificationType.Information));
    }    
    
    public void ShowError(string title, string message)
    {
        ShowNotification(new NotificationMessage(title, message, NotificationType.Error));
    }
    
    public void ShowNotification(NotificationMessage message)
    {
        _logger.LogInformation("Showing notification: {Title} - {Message} ({Severity})", message.Title, message.Message, message.NotificationType.ToString());
        _events.SendMessage(message);
    }
}