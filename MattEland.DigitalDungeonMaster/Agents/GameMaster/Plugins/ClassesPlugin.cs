using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Classes Plugin provides information about the playable character classes available in the game.")]
public class ClassesPlugin : GamePlugin
{
    private readonly IStorageService _storageService;

    // TODO: Not all rulesets will need this plugin
    
    public ClassesPlugin(RequestContextService context, IStorageService storageService) : base(context)
    {
        _storageService = storageService;
    }
    
    [KernelFunction("GetClasses")]
    [Description("Gets a list of available classes in the game.")]
    [return: Description("A list of classes characters can play as")]
    public IEnumerable<PlayerClassSummary> GetClasses()
    {
        string ruleset = Context.CurrentRuleset!;
        Context.LogPluginCall($"Ruleset: {ruleset}");
        
        // TODO: This mapping should be done in the storage service
        return _storageService.GetPartitionedDataAsync("classes", ruleset, e => new PlayerClassSummary
        {
            Name = (string)e["RowKey"]!,
            Description = (string)e["Description"]!
        }).Result;
    }

    public class PlayerClassSummary {
        public required string Name { get; init; }
        public required string Description { get; init; }
    }
}