namespace MattEland.BasementsAndBasilisks.Models;

public class BasiliskKernelConfig
{
    public string InitialPrompt { get; init; }
    public IEnumerable<BasiliskAgentConfig> Agents { get; init; }
}