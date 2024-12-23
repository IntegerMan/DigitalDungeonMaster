using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Services;
using MattEland.DigitalDungeonMaster.ClientShared;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public class HomeViewModel : ObservableObject
{
    private readonly EventsService _events;
    private readonly ApiClient _client;
    private readonly NotificationService _notify;

    public HomeViewModel(EventsService events, ApiClient client, NotificationService notify)
    {
        _events = events;
        _client = client;
        _notify = notify;

        NewGameCommand = new RelayCommand(StartNewGame);
        LoadGameCommand = new RelayCommand(LoadGame);
        LogoutCommand = new RelayCommand(Logout);
    }

    private void StartNewGame()
    {
        _events.SendMessage(new NavigateMessage(NavigateTarget.LoadGame));
    }
    
    private void LoadGame()
    {
        _events.SendMessage(new NavigateMessage(NavigateTarget.LoadGame));
    }    
    
    private void Logout()
    {
        string username = _client.Username;
        _client.Logout();
        _events.SendMessage(new LoggedOutMessage(username));
        _notify.ShowInfo("Logged out", "You have been logged out");
    }
    
    public RelayCommand NewGameCommand { get; }
    public RelayCommand LoadGameCommand { get; }
    public RelayCommand LogoutCommand { get; }
}