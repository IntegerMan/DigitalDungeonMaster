namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LoginViewModel _login;

    public MainWindowViewModel()
    {
        _login = App.GetService<LoginViewModel>();
        CurrentPage = _login;
    }

    /// <summary>
    /// The current page being displayed
    /// </summary>
    public ViewModelBase CurrentPage { get; }
}