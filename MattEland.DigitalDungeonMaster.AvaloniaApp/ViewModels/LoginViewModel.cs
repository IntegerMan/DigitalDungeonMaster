using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Services;
using MattEland.DigitalDungeonMaster.ClientShared;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public partial class LoginViewModel : ObservableValidator
{
    private readonly ILogger<LoginViewModel> _logger;
    private readonly ApiClient _client;
    private readonly NotificationService _notify;
    private readonly EventsService _events;

    public LoginViewModel(ILogger<LoginViewModel> logger, ApiClient client, NotificationService notify, EventsService events)
    {
        _logger = logger;
        _client = client;
        _notify = notify;
        _events = events;
        LoginCommand = new AsyncRelayCommand<LoginViewModel>(LoginAsync); // This could check IsValid, but that was unreliable on update
    }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Username is required")]
    [MinLength(5, ErrorMessage = "Username must be at least 5 characters")]
    [MaxLength(32, ErrorMessage = "Username must be 32 characters or less")]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    /// <summary>
    /// The command that should be executed on login attempt
    /// </summary>
    public AsyncRelayCommand<LoginViewModel> LoginCommand { get; }
    
    public bool IsValid => !HasErrors;

    [ObservableProperty]
    private bool _isBusy;

    private Task<ApiResult> LoginAsync(LoginViewModel? vm)
    {
        if (vm == null)
        {
            _logger.LogWarning($"{nameof(LoginAsync)} invoked with null view model");
            return Task.FromResult(ApiResult.Failure("Invalid view model"));
        }

        ValidateAllProperties();
        if (HasErrors)
        {
            string errors = string.Join(", ", GetErrors().Select(e => e.ErrorMessage));
            _logger.LogWarning("Login attempt failed due to validation errors: {Errors}", errors);
            _notify.ShowWarning("Validation Error", "Please correct the following issues: " + errors);
            return Task.FromResult(ApiResult.Failure("Validation errors: " + errors));
        }
        
        _logger.LogInformation("Logging in as {Username}", vm.Username);
        IsBusy = true;
        Task<ApiResult> loginResult = _client.LoginAsync(vm.Username, vm.Password);
        
        // We should return a Task so the UI can await it. We add a continuation to handle the result.
        return loginResult.ContinueWith(t =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                IsBusy = false;
                
                // Notify the UI that the IsValid property has changed - this is helpful when login fails
                OnPropertyChanged(nameof(IsValid));
                LoginCommand.NotifyCanExecuteChanged();
            });
            
            string message;
            if (!t.IsCompletedSuccessfully)
            {
                _logger.LogError(t.Exception, "Login failed for {Username}: {ErrorMessage}", vm.Username, t.Exception?.Message);
                message = "An error occurred during login";
                _notify.ShowError("Login Error", message);
                return ApiResult.Failure(message);
            }
            
            if (t.Result.Success)
            {
                // Clear the form for next time around
                string username = vm.Username;
                Dispatcher.UIThread.Invoke(() =>
                {
                    vm.Username = string.Empty;
                    vm.Password = string.Empty;
                });

                _logger.LogInformation("Login succeeded for {Username}", username);
                _notify.ShowSuccess("Login Successful", $"Welcome back, {username}");
                _events.SendMessage(new LoggedInMessage(username));
            }
            else
            {
                message = t.Result.ErrorMessage ?? "An unknown error occurred during login";
                _notify.ShowWarning("Login Failure", message);
                _logger.LogWarning("Login failed for {Username}: {ErrorMessage}", vm.Username, t.Result.ErrorMessage);
            }

            return t.Result;
        });;
    }
}