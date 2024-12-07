using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Skills Plugin provides information about the skills available in the game.")]
public class SkillsPlugin : GamePlugin
{
    private readonly StorageDataService _storageService;

    // TODO: May not be relevant to all rulesets
    
    public SkillsPlugin(RequestContextService context, StorageDataService storageService) : base(context)
    {
        _storageService = storageService;
    }
    
    [KernelFunction("GetSkills")]
    [Description("Gets a list of skills in the game and their uses.")]
    [return: Description("A list of skills and their uses")]
    public async Task<IEnumerable<SkillSummary>> GetSkillsAsync()
    {
        string ruleset = Context.CurrentRuleset!;
        Context.LogPluginCall($"Ruleset: {ruleset}");
        
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