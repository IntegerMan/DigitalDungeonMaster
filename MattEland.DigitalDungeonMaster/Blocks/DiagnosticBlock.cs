namespace MattEland.DigitalDungeonMaster.Blocks;

public class DiagnosticBlock : ChatBlockBase
{
    public string? Metadata { get; init; }
    public required string Header { get; set; }
}