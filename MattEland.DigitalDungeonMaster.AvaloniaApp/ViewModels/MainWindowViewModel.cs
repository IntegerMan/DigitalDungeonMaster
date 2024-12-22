﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IRecipient<LoggedInMessage>
{
    private readonly LoginViewModel _login;
    private readonly HomePageViewModel _home;

    public MainWindowViewModel()
    {
        _login = App.GetService<LoginViewModel>();
        _home = App.GetService<HomePageViewModel>();
        
        CurrentPage = _login;
        
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this);
    }

    /// <summary>
    /// The current page being displayed
    /// </summary>
    [ObservableProperty]
    private ViewModelBase currentPage;

    public void Receive(LoggedInMessage message)
    {
        CurrentPage = _home;
    }
}