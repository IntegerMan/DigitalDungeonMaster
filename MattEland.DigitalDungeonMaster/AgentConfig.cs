namespace MattEland.DigitalDungeonMaster;

public class AgentConfig
{
    public string NewCampaignPrompt { get; set; } = string.Empty;
    public string ResumeCampaignPrompt { get; set; } = string.Empty;
    public string MainPrompt { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AdditionalPrompt { get; set; }
    
    public string FullPrompt => $"{MainPrompt}\n\n{AdditionalPrompt}".Trim();
}