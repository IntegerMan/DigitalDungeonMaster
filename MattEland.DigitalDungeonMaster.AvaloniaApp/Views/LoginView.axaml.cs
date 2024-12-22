using Avalonia.Controls;
using MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        
        this.DataContext = App.Current.Services.GetRequiredService<LoginViewModel>();
    }
}