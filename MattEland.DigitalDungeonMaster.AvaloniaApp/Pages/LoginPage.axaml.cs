using Avalonia;
using Avalonia.Controls;
using MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Pages;

public partial class LoginPage : UserControl
{
    private readonly ILogger<LoginPage> _logger;

    public LoginPage()
    {
        _logger = App.GetService<ILogger<LoginPage>>();
        InitializeComponent();
        
        this.DataContext = App.GetService<LoginViewModel>();
    }

    private void FocusDefaultControl(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
        {
            _logger.LogDebug("Focusing control {Control}", control);
            control.Focus();
        }
    }
}