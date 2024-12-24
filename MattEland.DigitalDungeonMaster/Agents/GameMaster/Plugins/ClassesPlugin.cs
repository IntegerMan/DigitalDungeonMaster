using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Classes Plugin provides information about the playable character classes available in the game.")]
public class ClassesPlugin : PluginBase
{
    private readonly IRecordStorageService _recordStorage;
    private readonly RequestContextService _context;

    // TODO: Not all rulesets will need this plugin
    
    public ClassesPlugin(IRecordStorageService recordStorage, RequestContextService context, ILogger<ClassesPlugin> logger)
    : base(logger)
    {
        _recordStorage = recordStorage;
        _context = context;
    }
    
    [KernelFunction("GetClasses")]
    [Description("Gets a list of available classes in the game.")]
    [return: Description("A list of classes characters can play as")]
    public async Task<IEnumerable<PlayerClassSummary>> GetClasses()
    {
        string ruleset = _context.CurrentRuleset!;
        using Activity? activity = LogActivity($"Ruleset: {ruleset}");
        
        // TODO: This mapping should be done in the storage service
        List<PlayerClassSummary> result = (await _recordStorage.GetPartitionedDataAsync("classes", ruleset, e => new PlayerClassSummary
        {
            Name = (string)e["RowKey"]!,
            Description = (string)e["Description"]!
        })).ToList();
        
        foreach (var summary in result)
        {
            activity?.AddTag(summary.Name, summary.Description);
        }
        
        return result;
    }

    public class PlayerClassSummary {
        public required string Name { get; init; }
        public required string Description { get; init; }
        
        public override string ToString() => $"{Name}: {Description}";
    }
}