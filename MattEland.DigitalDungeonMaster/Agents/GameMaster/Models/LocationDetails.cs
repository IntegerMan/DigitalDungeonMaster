namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Models;

public class LocationDetails
{
    public LocationInfo Location { get; set; } = new() { Region = "Default", X = 0, Y = 0 };
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? GameHistory { get; set; } = string.Empty;
    public string? StorytellerNotes { get; set; } = string.Empty;

    public override string ToString() => $"{Name} ({Location}): {Description}";
}