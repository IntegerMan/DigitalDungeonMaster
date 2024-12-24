using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Attributes Plugin provides information about the player stats and attributes available in the game.")]
public class AttributesPlugin : PluginBase
{
    private readonly IRecordStorageService _recordStorage;
    private readonly RequestContextService _context;

    // TODO: May not be relevant to all rulesets
    
    public AttributesPlugin(IRecordStorageService recordStorage, RequestContextService context, ILogger<AttributesPlugin> logger)
        : base(logger)
    {
        _recordStorage = recordStorage;
        _context = context;
    }
    
    [KernelFunction("GetAttributes")]
    [Description("Gets a list of attributes in the game and their uses.")]
    [return: Description("A list of attributes and their uses")]
    public async Task<IEnumerable<AttributeSummary>> GetAttributesAsync()
    {
        string ruleset = _context.CurrentRuleset!;
        using Activity? activity = LogActivity($"Ruleset: {ruleset}");
        
        // TODO: This mapping should be done in the storage service
        List<AttributeSummary> summaries = (await _recordStorage.GetPartitionedDataAsync("attributes", ruleset, e => new AttributeSummary
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