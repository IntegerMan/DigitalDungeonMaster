using MattEland.DigitalDungeonMaster.Blocks;
using MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;

namespace MattEland.DigitalDungeonMaster.ConsoleApp.BlockRenderers;

public static class MessageBlockRenderer
{
    public static void Render(MessageBlock block)
    {
        if (block.IsUserMessage) return;

        DisplayHelpers.SayDungeonMasterLine(block.Message);
    }
}