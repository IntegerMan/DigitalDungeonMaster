using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated via Dependency Injection")]
[Description("The Attributes Plugin provides information about the player stats and attributes available in the game.")]
public class AttributesPlugin(
    IRecordStorageService recordStorage,
    RequestContextService context,
    ILogger<AttributesPlugin> logger)
    : PluginBase(logger)
{
    // TODO: May not be relevant to all rulesets

    [KernelFunction("GetAttributes")]
    [Description("Gets a list of attributes in the game and their uses.")]
    [return: Description("A list of attributes and their uses")]
    public async Task<IEnumerable<AttributeSummary>> GetAttributesAsync()
    {
        string ruleset = context.CurrentRuleset!;
        using Activity? activity = LogActivity($"Ruleset: {ruleset}");
        
        // TODO: This mapping should be done in the storage service
        List<AttributeSummary> summaries = (await recordStorage.GetPartitionedDataAsync("attributes", ruleset, e => new AttributeSummary
        {
            Name = (string)e["RowKey"]!,
            Description = (string)e["Description"]!
        })).ToList();
        
        foreach (AttributeSummary summary in summaries)
        {
            activity?.AddTag(summary.Name, summary.Description);
        }
        
        return summaries; 
    }

    public class AttributeSummary {
        public required string Name { get; set; }
        public required string Description { get; set; }
        
        public override string ToString() => $"{Name}: {Description}";
    }
}