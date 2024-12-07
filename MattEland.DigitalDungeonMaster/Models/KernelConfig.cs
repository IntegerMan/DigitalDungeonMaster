namespace MattEland.DigitalDungeonMaster.Models;

public class KernelConfig
{
    public string InitialPrompt { get; init; }
    public IEnumerable<GameAgentConfig> Agents { get; init; }
}