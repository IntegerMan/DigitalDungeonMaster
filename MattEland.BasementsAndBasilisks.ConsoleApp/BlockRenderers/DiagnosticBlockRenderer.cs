using MattEland.BasementsAndBasilisks.Blocks;

namespace MattEland.BasementsAndBasilisks.ConsoleApp.BlockRenderers;

public class DiagnosticBlockRenderer
{
    public static void Render(DiagnosticBlock block)
    {
        Panel panel = new Panel(block.Metadata ?? "No Metadata")
            .Header(block.Header)
            .Expand()
            .BorderStyle(new Style().Foreground(Color.Orange3));
        
        AnsiConsole.Write(panel);
    }
}