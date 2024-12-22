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

    private void InitializeNotifications()
    {
        _logger.LogDebug("Initializing notification manager");
        
        TopLevel? root = GetTopLevel(this);
        if (root == null)
        {
            _logger.LogWarning("Failed to get top level for notification manager");
            return;
        }
        _manager = new WindowNotificationManager(root)
        {
            MaxItems = 3
        };
        _logger.LogDebug("Created notification manager");
    }

    public void Receive(NotificationMessage notification)
    {
        // Ensure on the UI thread - plenty of async operations can trigger this
        if (!Dispatcher.UIThread.CheckAccess())
        {
            _logger.LogDebug("Received notification message on non-UI thread. Invoking on UI thread.");
            Dispatcher.UIThread.Invoke(() => Receive(notification));
            return;
        }
        
        _logger.LogInformation("Showing notification: {Title} - {Message} ({Severity})", notification.Title, notification.Message, notification.NotificationType.ToString());
        _manager.Show(new Notification(notification.Title, notification.Message, notification.NotificationType));
    }
}