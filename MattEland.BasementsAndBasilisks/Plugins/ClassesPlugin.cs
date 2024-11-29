using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "Classes")]
public class ClassesPlugin
{
    private readonly RequestContextService _context;
    private readonly StorageDataService _storageService;

    public ClassesPlugin(RequestContextService context, StorageDataService storageService)
    {
        _context = context;
        _storageService = storageService;
    }
    
    [KernelFunction("GetClasses")]
    [Description("Gets a list of available classes in the game.")]
    [return: Description("A list of classes characters can play as")]
    public IEnumerable<PlayerClassSummary> GetClasses()
    {
        string ruleset = _context.CurrentRuleset!;
        _context.LogPluginCall($"Ruleset: {ruleset}");
        
        return _storageService.ListTableEntriesInPartitionAsync("classes", ruleset, e => new PlayerClassSummary
        {
            Name = e.RowKey,
            Description = e.GetString("Description")
        }).Result;
    }

    public class PlayerClassSummary {
        public required string Name { get; init; }
        public required string Description { get; init; }
    }
}