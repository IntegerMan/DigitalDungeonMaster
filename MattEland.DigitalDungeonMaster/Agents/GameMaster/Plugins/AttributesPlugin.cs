using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Attributes Plugin provides information about the player stats and attributes available in the game.")]
public class AttributesPlugin : GamePlugin
{
    private readonly IStorageService _storageService;

    // TODO: May not be relevant to all rulesets
    
    public AttributesPlugin(RequestContextService context, IStorageService storageService) : base(context)
    {
        _storageService = storageService;
    }
    
    [KernelFunction("GetAttributes")]
    [Description("Gets a list of attributes in the game and their uses.")]
    [return: Description("A list of attributes and their uses")]
    public async Task<IEnumerable<AttributeSummary>> GetAttributesAsync()
    {
        string ruleset = Context.CurrentRuleset!;
        Context.LogPluginCall($"Ruleset: {ruleset}");
        
        // TODO: This mapping should be done in the storage service
        return await _storageService.GetPartitionedDataAsync("attributes", ruleset, e => new AttributeSummary
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