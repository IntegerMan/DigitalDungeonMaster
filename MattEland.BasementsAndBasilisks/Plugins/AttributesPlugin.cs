using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "Attributes")]
public class AttributesPlugin
{
    private readonly RequestContextService _context;
    private readonly StorageDataService _storageService;

    public AttributesPlugin(RequestContextService context, StorageDataService storageService)
    {
        _context = context;
        _storageService = storageService;
    }
    
    [KernelFunction("GetAttributes")]
    [Description("Gets a list of attributes in the game and their uses.")]
    [return: Description("A list of attributes and their uses")]
    public async Task<IEnumerable<AttributeSummary>> GetAttributes(string ruleset)
    {
        _context.LogPluginCall($"Ruleset: {ruleset}");
        
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