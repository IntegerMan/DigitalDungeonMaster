using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Classes Plugin provides information about the playable character classes available in the game.")]
public class ClassesPlugin
{
    private readonly IRecordStorageService _recordStorage;
    private readonly RequestContextService _context;
    private readonly ILogger<ClassesPlugin> _logger;

    // TODO: Not all rulesets will need this plugin
    
    public ClassesPlugin(IRecordStorageService recordStorage, RequestContextService context, ILogger<ClassesPlugin> logger)
    {
        _recordStorage = recordStorage;
        _context = context;
        _logger = logger;
    }
    
    [KernelFunction("GetClasses")]
    [Description("Gets a list of available classes in the game.")]
    [return: Description("A list of classes characters can play as")]
    public IEnumerable<PlayerClassSummary> GetClasses()
    {
        string ruleset = _context.CurrentRuleset!;
        _logger.LogDebug("{Plugin}-{Method} called under ruleset {Ruleset}", nameof(ClassesPlugin), nameof(GetClasses), ruleset);
        
        // TODO: This mapping should be done in the storage service
        return _recordStorage.GetPartitionedDataAsync("classes", ruleset, e => new PlayerClassSummary
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