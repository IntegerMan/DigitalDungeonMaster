using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
using MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Views;

public partial class MainWindow : Window, IRecipient<NotificationMessage>
{
    private WindowNotificationManager _manager;
    private readonly ILogger<MainWindow> _logger;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.GetService<MainWindowViewModel>();
        _logger = App.GetService<ILogger<MainWindow>>();
        InitializeNotifications();
        WeakReferenceMessenger.Default.Register(this);
    }

    [MemberNotNull(nameof(_manager))]
    private void InitializeNotifications()
    {
        _logger.LogTrace("Initializing notification manager");
        
        TopLevel root = GetTopLevel(this) ?? throw new InvalidOperationException("Failed to get top level window");

        _manager = new WindowNotificationManager(root)
        {
            MaxItems = 3
        };
        
        _logger.LogTrace("Created notification manager");
    }

    public void Receive(NotificationMessage notification)
    {
        // Ensure on the UI thread - plenty of async operations can trigger this
        if (!Dispatcher.UIThread.CheckAccess()) // TODO: Is there a shorthand for this via attributes?
        {
            _logger.LogTrace("Received notification message on non-UI thread. Invoking on UI thread.");
            Dispatcher.UIThread.Invoke(() =>
            {
                _logger.LogTrace("Now on UI thread. Invoking Receive.");
                Receive(notification);
            });
            return;
        }
        
        // NOTE: This could be expanded to include timeouts, positions, action on close, action on click, etc.
        _logger.LogInformation("Showing notification: {Title} - {Message} ({Severity})", notification.Title, notification.Message, notification.NotificationType.ToString());
        _manager.Show(new Notification(notification.Title, notification.Message, notification.NotificationType));
    }
}