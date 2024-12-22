using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
using MattEland.DigitalDungeonMaster.ClientShared;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public partial class LoginViewModel : ViewModelBase // TODO: Investigate ObservableValidator
{
    private readonly ILogger<LoginViewModel> _logger;
    private readonly ApiClient _client;

    public LoginViewModel(ILogger<LoginViewModel> logger, ApiClient client)
    {
        _logger = logger;
        _client = client;
        LoginCommand = new AsyncRelayCommand<LoginViewModel>(LoginAsync, _ => IsValid);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string username;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string password;

    /// <summary>
    /// The command that should be executed on login attempt
    /// </summary>
    public AsyncRelayCommand<LoginViewModel> LoginCommand { get; }
    
    public bool IsValid 
        => !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);

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
                _logger.LogDebug("Sending LoggedInMessage for {Username}", vm.Username);
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