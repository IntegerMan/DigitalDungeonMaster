using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _password = string.Empty;
    private string _username = string.Empty;
    private readonly ILogger<LoginViewModel> _logger;

    public LoginViewModel(ILogger<LoginViewModel> logger)
    {
        _logger = logger;
        LoginCommand = new RelayCommand<LoginViewModel>(Login, _ => IsValid);
    }

    /// <summary>
    /// The username to use for login
    /// </summary>
    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                IsValidChanged();
            }
        }
    }

    private void IsValidChanged()
    {
        OnPropertyChanged(nameof(IsValid));
        LoginCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// The password to use for login
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                IsValidChanged();
            }
        }
    }

    /// <summary>
    /// The command that should be executed on login attempt
    /// </summary>
    public RelayCommand<LoginViewModel> LoginCommand { get; }

    private void Login(LoginViewModel? vm)
    {
        if (vm == null)
        {
            _logger.LogWarning($"{nameof(Login)} invoked with null view model");
            return;
        }
        
        _logger.LogInformation("Logging in as {Username}", vm.Username);
    }

    public bool IsValid 
        => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}