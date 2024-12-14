using System.Text;

namespace MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;

public class NewGameSettingInfo
{
    public string PlayerCharacterName { get; set; } = string.Empty;
    public string PlayerDescription { get; set; } = string.Empty;
    public string PlayerCharacterClass { get; set; } = string.Empty;
    public string GameSettingDescription { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;
    public string CampaignObjective { get; set; } = string.Empty;
    public string FirstSessionObjective { get; set; } = string.Empty;
    public string DesiredGameplayStyle { get; set; } = string.Empty;

    public string Validate()
    {
        StringBuilder sb = new();
        
        if (string.IsNullOrWhiteSpace(PlayerCharacterName))
        {
            sb.AppendLine("Player character name is missing");
        }
        
        if (string.IsNullOrWhiteSpace(PlayerDescription))
        {
            sb.AppendLine("Player description is missing");
        }        
        
        if (string.IsNullOrWhiteSpace(PlayerCharacterClass))
        {
            sb.AppendLine("Player character class (barbarian, rogue, trader, etc.) is missing");
        }
        
        if (string.IsNullOrWhiteSpace(GameSettingDescription))
        {
            sb.AppendLine("Game setting description is missing");
        }
        
        if (string.IsNullOrWhiteSpace(CampaignName))
        {
            sb.AppendLine("Campaign name is missing");
        }        
        
        if (string.IsNullOrWhiteSpace(CampaignObjective))
        {
            sb.AppendLine("Campaign objective is missing");
        }
        
        if (string.IsNullOrWhiteSpace(DesiredGameplayStyle))
        {
            sb.AppendLine("Desired gameplay style is missing");
        }        
        
        if (string.IsNullOrWhiteSpace(FirstSessionObjective))
        {
            sb.AppendLine("First session objective is missing");
        }
                
        return sb.ToString();
    }
}