using MattEland.DigitalDungeonMaster.Blocks;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.BlockRenderers;

public class TextResourceBlockRenderer
{
    public static void Render(TextResourceBlock block)
    {
        Panel panel = new Panel(block.Content)
            .Header(block.Title)
            .Expand()
            .BorderStyle(new Style().Foreground(Color.Cyan3));
        
        AnsiConsole.Write(panel);
    }
}