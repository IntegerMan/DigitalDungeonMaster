using MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Messages;

public class NavigateMessage
{
    public NavigateTarget Target { get; }

    public NavigateMessage(NavigateTarget target)
    {
        Target = target;
    }
    
    public override string ToString() => $"Navigating to {Target}";
}