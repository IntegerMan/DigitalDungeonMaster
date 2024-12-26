using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated via Dependency Injection")]
[Description("The Skills Plugin provides information about the skills available in the game.")]
public class SkillsPlugin(
    RequestContextService context,
    IRecordStorageService storageService,
    ILogger<SkillsPlugin> logger)
    : PluginBase(logger)
{
    // TODO: May not be relevant to all rulesets

    [KernelFunction("GetSkills")]
    [Description("Gets a list of skills in the game and their uses.")]
    [return: Description("A list of skills and their uses")]
    public async Task<IEnumerable<SkillSummary>> GetSkillsAsync()
    {
        string ruleset = context.CurrentRuleset!;
        using Activity? activity = LogActivity($"Ruleset: {ruleset}");
        
        // TODO: This mapping should be done in the storage service
        return await storageService.GetPartitionedDataAsync("skills", ruleset, e => new SkillSummary
        {
            Name = (string)e["RowKey"]!,
            Description = (string)e["Description"]!,
            Attribute = e["Attribute"] as string
        });
    }

    public class SkillSummary {
        public required string Name { get; set; }
        public string? Attribute { get; set; }
        
        public required string Description { get; set; }
        
        public override string ToString() => $"{Name} ({Attribute ?? "No Attribute"}): {Description}";
    }
}