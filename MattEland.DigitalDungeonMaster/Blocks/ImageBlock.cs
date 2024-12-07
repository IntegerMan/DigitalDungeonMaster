namespace MattEland.DigitalDungeonMaster.Blocks;

public class ImageBlock : ChatBlockBase
{
    public string Filename { get; }
    public string? Description { get; }
    
    public ImageBlock(string filename, string? description)
    {
        Filename = filename;
        Description = description;
    }

    public override string ToString() => Description ?? Filename;
}