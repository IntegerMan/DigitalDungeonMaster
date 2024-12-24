using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;
using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.Agents.WorldBuilder.Plugins;

[Description("Plugin for creating a new game setting")]
public class SettingCreationPlugin : PluginBase
{
    public SettingCreationPlugin(ILogger<SettingCreationPlugin> logger) : base(logger)
    {
        SettingInfo = new();
    }
    
    [KernelFunction("GetCurrentSettingInfo"), Description("Gets the current setting information")]
    public NewGameSettingInfo GetCurrentSettingInfo() => SettingInfo;

    [KernelFunction("SetPlayerCharacterName"), Description("Sets the player character name")]
    public string SetPlayerCharacterName([Description("The name of the player")] string name)
    {
        using Activity? activity = LogActivity($"Setting player character name to {name}");
        
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name cannot be empty";
        }
        
        SettingInfo.PlayerCharacterName = name;
        
        return $"Player character name set to {name}";
    }
    
    [KernelFunction("SetPlayerDescription"), Description("Sets the player character description")]
    public string SetPlayerDescription([Description("The description of the player")] string description)
    {
        using Activity? activity = LogActivity($"Setting player description to {description}");
        
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Description cannot be empty";
        }
        
        SettingInfo.PlayerDescription = description;
        
        return $"Player description set to {description}";
    }    
    
    [KernelFunction("SetPlayerRole"), Description("Sets the player character's class, profession, or role. This determines their playstyle.")]
    public string SetPlayerRole([Description("The role or starting class of the player")] string role)
    {
        using Activity? activity = LogActivity($"Setting player role to {role}");
        
        if (string.IsNullOrWhiteSpace(role))
        {
            return "Player role cannot be empty";
        }
        
        SettingInfo.PlayerCharacterClass = role;
        
        return $"Player character class / role set to {role}";
    }
    
    [KernelFunction("SetGameSettingDescription"), Description("Sets the game setting description")]
    public string SetGameSettingDescription([Description("The description of the game setting")] string description)
    {
        using Activity? activity = LogActivity($"Setting game setting description to {description}");
        
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Description cannot be empty";
        }
        
        SettingInfo.GameSettingDescription = description;
        
        return $"Game setting description set to {description}";
    }
    
    [KernelFunction("SetCampaignName"), Description("Sets the campaign name")]
    public string SetCampaignName([Description("The name of the campaign")] string name)
    {
        using Activity? activity = LogActivity($"Setting campaign name to {name}");
        
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name cannot be empty";
        }
        
        SettingInfo.CampaignName = name;
        
        return $"Campaign name set to {name}";
    }
    
    [KernelFunction("SetCampaignObjective"), Description("Sets the campaign objective")]
    public string SetCampaignObjective([Description("The objective of the campaign")] string objective)
    {
        using Activity? activity = LogActivity($"Setting campaign objective to {objective}");
        
        if (string.IsNullOrWhiteSpace(objective))
        {
            return "Objective cannot be empty";
        }
        
        SettingInfo.CampaignObjective = objective;
        
        return $"Campaign objective set to {objective}";
    }    
    
    [KernelFunction("SetFirstSessionObjective"), Description("Sets the details of the first session")]
    public string SetFirstSessionObjective([Description("The objective of the first session")] string objective)
    {
        using Activity? activity = LogActivity($"Setting first session objective to {objective}");
        
        if (string.IsNullOrWhiteSpace(objective))
        {
            return "Objective cannot be empty";
        }
        
        SettingInfo.FirstSessionObjective = objective;
        
        return $"First session objective set to {objective}";
    }
    
    [KernelFunction("SetDesiredGameplayStyle"), Description("Sets the desired gameplay style")]
    public string SetDesiredGameplayStyle([Description("The desired gameplay style")] string style)
    {
        using Activity? activity = LogActivity($"Setting desired gameplay style to {style}");
        
        if (string.IsNullOrWhiteSpace(style))
        {
            return "Style cannot be empty";
        }
        
        SettingInfo.DesiredGameplayStyle = style;
        
        return $"Desired gameplay style set to {style}";
    }
    
    [KernelFunction("ConfirmSettingInfo"), Description("Confirms the setting information and marks the game as ready to begin")]
    public string ConfirmSettingInfo()
    {
        using Activity? activity = LogActivity("Confirming setting information");
        
        if (!SettingInfo.IsValid)
        {
            return "Setting information is incomplete: " + SettingInfo.Validate();
        }
        
        SettingInfo.IsConfirmed = true;
        
        return "Setting information confirmed. Tell the player the game will begin shortly.";
    }
    
    [KernelFunction("ValidateSettingInfo"), Description("Checks the current setting info for completion and returns any issues found")]
    public string ValidateSettingInfo()
    {
        using Activity? activity = LogActivity("Validating setting information");
        
        string issues = SettingInfo.Validate();

        if (issues.Length == 0)
        {
            return "Setting information is valid. Ask the player to confirm and then call " + nameof(BeginAdventure);
        }
        
        return issues;
    }

    [KernelFunction("BeginAdventure"), Description("Marks the setting information as complete and begins the story.")]
    public string BeginAdventure()
    {
        using Activity? activity = LogActivity("Beginning the adventure");
        
        string issues = SettingInfo.Validate();
        if (issues.Length > 0)
        {
            return $"Setting information is incomplete: {issues}";
        }

        IsFinalized = true;
        
        return "Setting information finalized";
    }

    public bool IsFinalized { get; private set; }
    public NewGameSettingInfo SettingInfo { get; set; }
}