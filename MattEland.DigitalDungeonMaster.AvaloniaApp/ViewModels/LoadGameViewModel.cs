using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Services;
using MattEland.DigitalDungeonMaster.ClientShared;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public partial class LoadGameViewModel : ObservableObject
{
    private readonly ApiClient _client;
    private readonly ILogger<LoadGameViewModel> _logger;
    private readonly NotificationService _notify;
    private readonly EventsService _events;

    public LoadGameViewModel(ApiClient client, ILogger<LoadGameViewModel> logger, NotificationService notify, EventsService events)
    {
        _client = client;
        _logger = logger;
        _notify = notify;
        _events = events;

        LoadGameCommand = new RelayCommand(LoadSelectedGame);
        NavigateBackCommand = new RelayCommand(NavigateBack);
    }

    public RelayCommand NavigateBackCommand { get; set; }

    private void NavigateBack()
    {
        _logger.LogInformation("Navigating back to home");
        _events.SendMessage(new NavigateMessage(NavigateTarget.Home));
    }

    public RelayCommand LoadGameCommand { get; }
    
    [ObservableProperty]
    private AdventureInfo? _selectedAdventure;

    private void LoadSelectedGame()
    {
        AdventureInfo? adventure = SelectedAdventure;
        if (adventure == null)
        {
            _notify.ShowWarning("No Game Selected", "Please select a game to load");
            return;
        }

        _logger.LogInformation("Loading game {Game} ({Key})", adventure.Name, adventure.RowKey);
        _events.SendMessage(new GameLoadedMessage(adventure));
    }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private ObservableCollection<AdventureInfo> _adventures = new();

    public void LoadGames()
    {
        IsBusy = true;
        _client.LoadAdventuresAsync().ContinueWith(r =>
        {
            _logger.LogTrace("Load adventures complete. Moving to UI thread");
            Dispatcher.UIThread.Invoke(() =>
            {
                IsBusy = false;
                
                if (r.IsFaulted)
                {
                    _logger.LogError(r.Exception, "Failed to load games");
                    _notify.ShowError("Failed to load games", "An error occurred while loading games");
                    return;
                }
                
                List<AdventureInfo> adventures = r.Result?.ToList() ?? [];
                _notify.ShowSuccess("Adventures Loaded", $"Loaded {adventures.Count} Adventures");

                // Set our adventures into the collection
                Adventures.Clear();
                foreach (var adventure in adventures.Where(a => a.Status != AdventureStatus.Building))
                {
                    Adventures.Add(adventure);
                }
            });

        });
    }
}