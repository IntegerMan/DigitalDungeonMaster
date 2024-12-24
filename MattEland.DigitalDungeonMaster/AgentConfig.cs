namespace MattEland.DigitalDungeonMaster;

public class AgentConfig
{
    public string NewCampaignPrompt { get; init; } = string.Empty;
    public string ResumeCampaignPrompt { get; init; } = string.Empty;
    public string MainPrompt { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? AdditionalPrompt { get; set; }
    
    public string FullPrompt => $"{MainPrompt}\n\n{AdditionalPrompt}".Trim();
    public bool IsAvailableInGame { get; init; }
}