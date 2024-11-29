using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "Skills")]
public class SkillsPlugin
{
    private readonly RequestContextService _context;
    private readonly StorageDataService _storageService;

    public SkillsPlugin(RequestContextService context, StorageDataService storageService)
    {
        _context = context;
        _storageService = storageService;
    }
    
    [KernelFunction("GetSkills")]
    [Description("Gets a list of skills in the game and their uses.")]
    [return: Description("A list of skills and their uses")]
    public async Task<IEnumerable<SkillSummary>> GetSkillsAsync()
    {
        string ruleset = _context.CurrentRuleset;
        _context.LogPluginCall($"Ruleset: {ruleset}");
        
        return await _storageService.ListTableEntriesInPartitionAsync("skills", ruleset, e => new SkillSummary
        {
            Name = e.RowKey,
            Description = e.GetString("Description"),
            Attribute = e.GetString("Attribute")
        });
    }

    public class SkillSummary {
        public required string Name { get; set; }
        public required string Attribute { get; set; }
        
        public required string Description { get; set; }
    }
}