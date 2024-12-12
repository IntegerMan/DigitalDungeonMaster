namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Models;

public class LocationDetails
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GameHistory { get; set; } = string.Empty;
    public string PrivateStorytellerNotes { get; set; } = string.Empty;
}