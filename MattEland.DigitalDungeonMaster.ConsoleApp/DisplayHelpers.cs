using MattEland.DigitalDungeonMaster.Blocks;
using MattEland.DigitalDungeonMaster.ConsoleApp.BlockRenderers;

namespace MattEland.DigitalDungeonMaster.ConsoleApp;

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
            case ImageBlock image:
                ImageBlockRenderer.Render(image);
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

    public static void RenderHeader()
    {
        AnsiConsole.Write(new FigletText("Digital DM").Color(Color.Yellow));
        AnsiConsole.MarkupLine("AI Orchestration Game Master proof of concept by [SteelBlue]Matt Eland[/].");
        AnsiConsole.WriteLine();
    }
}