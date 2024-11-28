using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.ConsoleApp.BlockRenderers;

namespace MattEland.BasementsAndBasilisks.ConsoleApp;

public static class DisplayHelpers
{
    public static void SayDungeonMasterLine(string message)
    {
        Console.WriteLine();
        try
        {
            AnsiConsole.MarkupLine($"[SteelBlue]DM[/]: {message}");
        }
        catch (InvalidOperationException)
        {
            // Fallback to WriteLine in cases where response is not valid markup
            AnsiConsole.WriteLine($"DM: {message}");
        }
    }

    private static void Render(this ChatBlockBase block)
    {
        switch (block)
        {
            case MessageBlock message:
                MessageBlockRenderer.Render(message);
                break;
            case TextResourceBlock textResource:
                TextResourceBlockRenderer.Render(textResource);
                break;            
            case DiagnosticBlock diagnostic:
                DiagnosticBlockRenderer.Render(diagnostic);
                break;
            default:
                DefaultRenderer.Render(block);
                break;
        }
    }

    public static void Render(this IEnumerable<ChatBlockBase> blocks)
    {
        foreach (var block in blocks)
        {
            block.Render();
        }
    }
}