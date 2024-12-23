using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Pages;

public partial class LoadGamePage : UserControl
{
    public LoadGamePage()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        LoadGameViewModel vm = (LoadGameViewModel)DataContext!;
        
        vm.LoadGames();
    }
}