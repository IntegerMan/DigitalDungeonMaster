using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Attributes Plugin provides information about the player stats and attributes available in the game.")]
public class AttributesPlugin : GamePlugin
{
    private readonly StorageDataService _storageService;

    // TODO: May not be relevant to all rulesets
    
    public AttributesPlugin(RequestContextService context, StorageDataService storageService) : base(context)
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
        
        return await _storageService.ListTableEntriesInPartitionAsync("attributes", ruleset, e => new AttributeSummary
        {
            Name = e.RowKey,
            Description = e.GetString("Description")
        }); 
    }

    public class AttributeSummary {
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}