namespace MattEland.DigitalDungeonMaster.GameManagement.Models;

public class AdventureInfo
{
    public string RowKey { get; init; } = string.Empty;
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string Container { get; init; } = string.Empty;
    public string Ruleset { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
}