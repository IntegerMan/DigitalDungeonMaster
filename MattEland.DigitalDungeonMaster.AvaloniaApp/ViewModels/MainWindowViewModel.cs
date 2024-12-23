using System;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Services;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject, 
    IRecipient<LoggedInMessage>, 
    IRecipient<LoggedOutMessage>,
    IRecipient<NavigateMessage>,
    IRecipient<GameLoadedMessage>
{
    private readonly HomeViewModel _home;

    public MainWindowViewModel()
    {
        _home = App.GetService<HomeViewModel>();
        _notify = App.GetService<NotificationService>();
        
        CurrentPage = App.GetService<InGameViewModel>();
        
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    /// <summary>
    /// The current page being displayed
    /// </summary>
    [ObservableProperty]
    private ObservableObject _currentPage;

    private readonly NotificationService _notify;

    public void Receive(LoggedInMessage message)
    {
        ShowHome();
    }

    public void Receive(LoggedOutMessage message)
    {
        ShowLogin();
    }

    [MemberNotNull(nameof(_currentPage))]
    [SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0034:Direct field reference to [ObservableProperty] backing field")]
    private void ShowLogin()
    {
        CurrentPage = App.GetService<LoginViewModel>();
    }

    public void Receive(NavigateMessage message)
    {
        switch (message.Target)
        {
            case NavigateTarget.Home:
                ShowHome();
                break;
            case NavigateTarget.Login:
                ShowLogin();
                break;            
            case NavigateTarget.LoadGame:
                ShowLoadGame();
                break;            
            case NavigateTarget.NewGame:
                _notify.ShowWarning("New Game Not Implemented", "New games is not yet implemented in this client");
                break;
            default:
                _notify.ShowError("Unsupported Navigation Target", $"Navigation to {message.Target} is not supported in this client");
                throw new NotSupportedException($"Unsupported navigation target: {message.Target}");
        }
    }

    private void ShowLoadGame()
    {
        CurrentPage = App.GetService<LoadGameViewModel>();
    }

    private void ShowHome()
    {
        CurrentPage = _home;
    }

    public void Receive(GameLoadedMessage message)
    {
        CurrentPage = App.GetService<InGameViewModel>();
    }
}