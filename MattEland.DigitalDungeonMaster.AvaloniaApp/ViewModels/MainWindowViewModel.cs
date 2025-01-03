﻿using System;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Services;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.Logging;

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
        _logger = App.GetService<ILogger<MainWindowViewModel>>();
        
        ShowLogin();
        
        WeakReferenceMessenger.Default.RegisterAll(this);
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, RequestMessage<AdventureInfo>>(this,
            (vm, message) =>
            {
                if (vm.Adventure == null) throw new InvalidOperationException("No Adventure Set");
                
                vm._logger.LogDebug("Handling request for current adventure: {Adventure}", vm.Adventure.RowKey);
                message.Reply(vm.Adventure);
            });
    }

    /// <summary>
    /// The current page being displayed
    /// </summary>
    [ObservableProperty]
    private ObservableObject _currentPage;

    private readonly NotificationService _notify;
    
    [ObservableProperty]
    private AdventureInfo? _adventure;

    private readonly ILogger<MainWindowViewModel> _logger;

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
        Adventure = message.Adventure;
        CurrentPage = App.GetService<InGameViewModel>();
    }
}