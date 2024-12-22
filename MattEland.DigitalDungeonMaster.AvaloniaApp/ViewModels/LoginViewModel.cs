using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
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
        Task<ApiResult> loginResult = _client.LoginAsync(vm.Username, vm.Password);
        
        // We should return a Task so the UI can await it. We add a continuation to handle the result.
        return loginResult.ContinueWith(t =>
        {
            if (!t.IsCompletedSuccessfully)
            {
                _logger.LogError(t.Exception, "Login failed for {Username}: {ErrorMessage}", vm.Username, t.Exception?.Message);
                return ApiResult.Failure("Login failed due to an exception");
            }
            if (t.Result.Success)
            {
                _logger.LogInformation("Login succeeded for {Username}", vm.Username);
                WeakReferenceMessenger.Default.Send(new LoggedInMessage(vm.Username));
            }
            else
            {
                _logger.LogWarning("Login failed for {Username}: {ErrorMessage}", vm.Username, t.Result.ErrorMessage);
            }

            return t.Result;
        });;
    }
}