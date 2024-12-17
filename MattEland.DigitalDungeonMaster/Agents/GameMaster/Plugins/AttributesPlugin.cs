using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Attributes Plugin provides information about the player stats and attributes available in the game.")]
public class AttributesPlugin
{
    private readonly IRecordStorageService _recordStorage;
    private readonly RequestContextService _context;
    private readonly ILogger<AttributesPlugin> _logger;

    // TODO: May not be relevant to all rulesets
    
    public AttributesPlugin(IRecordStorageService recordStorage, RequestContextService context, ILogger<AttributesPlugin> logger)
    {
        _recordStorage = recordStorage;
        _context = context;
        _logger = logger;
    }
    
    [KernelFunction("GetAttributes")]
    [Description("Gets a list of attributes in the game and their uses.")]
    [return: Description("A list of attributes and their uses")]
    public async Task<IEnumerable<AttributeSummary>> GetAttributesAsync()
    {
        string ruleset = _context.CurrentRuleset!;
        _logger.LogDebug("{Plugin}-{Method} called under ruleset: {ruleset}", nameof(AttributesPlugin), nameof(GetAttributesAsync), ruleset);
        
        // TODO: This mapping should be done in the storage service
        return await _recordStorage.GetPartitionedDataAsync("attributes", ruleset, e => new AttributeSummary
        {
            Name = (string)e["RowKey"]!,
            Description = (string)e["Description"]!
        }); 
    }

    public class AttributeSummary {
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}