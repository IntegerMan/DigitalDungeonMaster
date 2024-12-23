using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Services;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject, 
    IRecipient<LoggedInMessage>, 
    IRecipient<LoggedOutMessage>,
    IRecipient<NavigateMessage>
{
    private readonly LoginViewModel _login;
    private readonly HomeViewModel _home;

    public MainWindowViewModel()
    {
        _login = App.GetService<LoginViewModel>();
        _home = App.GetService<HomeViewModel>();
        _notify = App.GetService<NotificationService>();
        
        CurrentPage = _login;
        
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this);
        WeakReferenceMessenger.Default.Register<LoggedOutMessage>(this);
        WeakReferenceMessenger.Default.Register<NavigateMessage>(this);
    }

    /// <summary>
    /// The current page being displayed
    /// </summary>
    [ObservableProperty]
    private ObservableObject _currentPage;

    private readonly NotificationService _notify;

    public void Receive(LoggedInMessage message)
    {
        CurrentPage = _home;
    }

    public void Receive(LoggedOutMessage message)
    {
        CurrentPage = _login;
    }

    public void Receive(NavigateMessage message)
    {
        switch (message.Target)
        {
            case NavigateTarget.Home:
                CurrentPage = _home;
                break;
            case NavigateTarget.Login:
                CurrentPage = _login;
                break;            
            case NavigateTarget.LoadGame:
                _notify.ShowWarning("Loading Not Implemented", "Loading games is not yet implemented in this client");
                break;            
            case NavigateTarget.NewGame:
                _notify.ShowWarning("New Game Not Implemented", "New games is not yet implemented in this client");
                break;
            default:
                _notify.ShowError("Unsupported Navigation Target", $"Navigation to {message.Target} is not supported in this client");
                throw new NotSupportedException($"Unsupported navigation target: {message.Target}");
        }
    }
}