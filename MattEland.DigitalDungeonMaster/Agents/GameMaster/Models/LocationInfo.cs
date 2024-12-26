namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Models;

public record LocationInfo
{
    public required string Region { get; set; } = "default";
    public int X { get; set; }
    public int Y { get; set; }
    
    public override string ToString() => $"{Region}_{X}_{Y})";
}