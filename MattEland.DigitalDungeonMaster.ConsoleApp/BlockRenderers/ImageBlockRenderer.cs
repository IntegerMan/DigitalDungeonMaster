using MattEland.DigitalDungeonMaster.Blocks;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.BlockRenderers;

public class ImageBlockRenderer
{
    public static void Render(ImageBlock block)
    {
        AnsiConsole.MarkupLineInterpolated($"[Yellow]{block.Filename}[/]");
        CanvasImage image = new CanvasImage(block.Filename);
        image.MaxWidth(32);
        AnsiConsole.Write(image);

        if (!string.IsNullOrWhiteSpace(block.Description))
        {
            AnsiConsole.MarkupLine($"[italic][SteelBlue]{block.Description}[/][/]");
            AnsiConsole.WriteLine();
        }
    }
}