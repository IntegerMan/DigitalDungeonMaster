using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Widgets;

public class PageHeader : ContentControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PageHeader, string>(nameof(Title), defaultValue: "Title");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}