using MattEland.BasementsAndBasilisks.Blocks;

namespace MattEland.BasementsAndBasilisks.ConsoleApp.BlockRenderers;

public static class MessageBlockRenderer
{
    public static void Render(MessageBlock block)
    {
        if (block.IsUserMessage) return;

        DisplayHelpers.SayDungeonMasterLine(block.Message);
    }
}