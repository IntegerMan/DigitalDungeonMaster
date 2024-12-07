using MattEland.DigitalDungeonMaster.Blocks;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.BlockRenderers;

public static class DefaultRenderer
{
    public static void Render(ChatBlockBase block)
    {
        string message = $"[Orange3]Unsupported Block:[/] {block.GetType().Name}: {block}";
        try
        {
            AnsiConsole.MarkupLine(message);
        } catch (InvalidOperationException) {
            AnsiConsole.WriteLine(message);
        }
    }
}