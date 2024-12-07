namespace MattEland.DigitalDungeonMaster.Models;

public class Ruleset
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Owner { get; set; }
    public required string Key { get; set; }
}