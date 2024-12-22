using Avalonia.Controls;
using MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.GetService<MainWindowViewModel>();
    }
}