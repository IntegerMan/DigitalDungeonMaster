using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MattEland.DigitalDungeonMaster.ClientShared;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _password = string.Empty;
    private string _username = string.Empty;
    private readonly ILogger<LoginViewModel> _logger;
    private readonly ApiClient _client;

    public LoginViewModel(ILogger<LoginViewModel> logger, ApiClient client)
    {
        _logger = logger;
        _client = client;
        LoginCommand = new AsyncRelayCommand<LoginViewModel>(LoginAsync, _ => IsValid);
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
    public AsyncRelayCommand<LoginViewModel> LoginCommand { get; }
    
    
    public bool IsValid 
        => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

    private Task<ApiResult> LoginAsync(LoginViewModel? vm)
    {
        if (vm == null)
        {
            _logger.LogWarning($"{nameof(LoginAsync)} invoked with null view model");
            return Task.FromResult(ApiResult.Failure("Invalid view model"));
        }
        
        _logger.LogInformation("Logging in as {Username}", vm.Username);
        return _client.LoginAsync(vm.Username, vm.Password);
    }
}