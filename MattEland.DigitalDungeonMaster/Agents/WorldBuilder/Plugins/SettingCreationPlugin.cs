using System.Text;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Models;

namespace MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Plugins;

[Description("Plugin for creating a new game setting")]
public class SettingCreationPlugin
{
    private readonly NewGameSettingInfo _info = new();
    
    [KernelFunction("GetCurrentSettingInfo"), Description("Gets the current setting information")]
    public NewGameSettingInfo GetCurrentSettingInfo() => _info;

    [KernelFunction("SetPlayerCharacterName"), Description("Sets the player character name")]
    public string SetPlayerCharacterName([Description("The name of the player")] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name cannot be empty";
        }
        
        _info.PlayerCharacterName = name;
        
        return $"Player character name set to {name}";
    }
    
    [KernelFunction("SetPlayerDescription"), Description("Sets the player character description")]
    public string SetPlayerDescription([Description("The description of the player")] string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Description cannot be empty";
        }
        
        _info.PlayerDescription = description;
        
        return $"Player description set to {description}";
    }    
    
    [KernelFunction("SetPlayerRole"), Description("Sets the player character's class, profession, or role. This determines their playstyle.")]
    public string SetPlayerRole([Description("The role or starting class of the player")] string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return "Player role cannot be empty";
        }
        
        _info.PlayerCharacterClass = role;
        
        return $"Player character class / role set to {role}";
    }
    
    [KernelFunction("SetGameSettingDescription"), Description("Sets the game setting description")]
    public string SetGameSettingDescription([Description("The description of the game setting")] string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Description cannot be empty";
        }
        
        _info.GameSettingDescription = description;
        
        return $"Game setting description set to {description}";
    }
    
    [KernelFunction("SetCampaignName"), Description("Sets the campaign name")]
    public string SetCampaignName([Description("The name of the campaign")] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name cannot be empty";
        }
        
        _info.CampaignName = name;
        
        return $"Campaign name set to {name}";
    }
    
    [KernelFunction("SetCampaignObjective"), Description("Sets the campaign objective")]
    public string SetCampaignObjective([Description("The objective of the campaign")] string objective)
    {
        if (string.IsNullOrWhiteSpace(objective))
        {
            return "Objective cannot be empty";
        }
        
        _info.CampaignObjective = objective;
        
        return $"Campaign objective set to {objective}";
    }    
    
    [KernelFunction("SetFirstSessionObjective"), Description("Sets the details of the first session")]
    public string SetFirstSessionObjective([Description("The objective of the first session")] string objective)
    {
        if (string.IsNullOrWhiteSpace(objective))
        {
            return "Objective cannot be empty";
        }
        
        _info.FirstSessionObjective = objective;
        
        return $"First session objective set to {objective}";
    }
    
    [KernelFunction("SetDesiredGameplayStyle"), Description("Sets the desired gameplay style")]
    public string SetDesiredGameplayStyle([Description("The desired gameplay style")] string style)
    {
        if (string.IsNullOrWhiteSpace(style))
        {
            return "Style cannot be empty";
        }
        
        _info.DesiredGameplayStyle = style;
        
        return $"Desired gameplay style set to {style}";
    }

    [KernelFunction("ValidateSettingInfo"), Description("Checks the current setting info for completion and returns any issues found")]
    public string ValidateSettingInfo()
    {
        string issues = _info.Validate();

        if (issues.Length == 0)
        {
            return "Setting information is valid. Ask the player to confirm and then call " + nameof(BeginAdventure);
        }
        
        return issues;
    }

    [KernelFunction("BeginAdventure"), Description("Marks the setting information as complete and begins the story.")]
    public string BeginAdventure()
    {
        string issues = _info.Validate();
        if (issues.Length > 0)
        {
            return $"Setting information is incomplete: {issues}";
        }

        IsFinalized = true;
        
        return "Setting information finalized";
    }

    public bool IsFinalized { get; private set; }
}