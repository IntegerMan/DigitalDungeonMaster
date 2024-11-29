namespace MattEland.BasementsAndBasilisks;

public class BasiliskKernelConfig
{
    public string InitialPrompt { get; init; }
    public IEnumerable<BasiliskAgentConfig> Agents { get; init; }
}