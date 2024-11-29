namespace MattEland.BasementsAndBasilisks.Models;

public class AdventureInfo
{
    public required string RowKey { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Container { get; init; }
    public required string Ruleset { get; init; }
    public required string GameWorld { get; init; }
}