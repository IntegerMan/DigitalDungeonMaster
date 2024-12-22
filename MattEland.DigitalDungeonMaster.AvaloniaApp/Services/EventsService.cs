using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Services;

public class EventsService
{
    private readonly ILogger<EventsService> _logger;

    public EventsService(ILogger<EventsService> logger)
    {
        _logger = logger;
    }
    
    public void SendMessage<T>(T message) where T : class
    {
        _logger.LogInformation("Sending {Type}: {Message}", typeof(T).Name, message);
        WeakReferenceMessenger.Default.Send(message);
    }
}