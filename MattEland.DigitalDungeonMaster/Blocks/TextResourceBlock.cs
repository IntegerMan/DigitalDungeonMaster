namespace MattEland.DigitalDungeonMaster.Blocks;

public class TextResourceBlock(string title, string content) : ChatBlockBase
{
    public string Title { get; } = title;
    public string Content { get; } = content;
}