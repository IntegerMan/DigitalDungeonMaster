namespace MattEland.BasementsAndBasilisks.Blocks;

public class DiagnosticBlock : ChatBlockBase
{
    public string? Metadata { get; init; }
    public required string Header { get; set; }
}