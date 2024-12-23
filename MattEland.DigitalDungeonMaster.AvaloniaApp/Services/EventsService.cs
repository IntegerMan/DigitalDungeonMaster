using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Services;

public class EventsService
{
    private readonly ILogger<EventsService> _logger;

    public EventsService(ILogger<EventsService> logger)
    {
        _logger = logger;
    }
    
    public T SendMessage<T>(T message) where T : class
    {
        _logger.LogDebug("Sending {Type}: {Message}", typeof(T).Name, message);
        
        return WeakReferenceMessenger.Default.Send(message);
    }

    public TResult Request<TResult>() 
    {
        _logger.LogDebug("Requesting {Type}", typeof(TResult).Name);

        return WeakReferenceMessenger.Default.Send<RequestMessage<TResult>>();
    }
}