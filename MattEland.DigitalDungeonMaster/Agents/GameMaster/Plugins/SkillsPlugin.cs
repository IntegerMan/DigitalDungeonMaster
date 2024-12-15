using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Skills Plugin provides information about the skills available in the game.")]
public class SkillsPlugin : GamePlugin
{
    private readonly IStorageService _storageService;

    // TODO: May not be relevant to all rulesets
    
    public SkillsPlugin(RequestContextService context, IStorageService storageService) : base(context)
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
        
        // TODO: This mapping should be done in the storage service
        return await _storageService.GetPartitionedDataAsync("skills", ruleset, e => new SkillSummary
        {
            Name = (string)e["RowKey"]!,
            Description = (string)e["Description"]!,
            Attribute = (string)e["Attribute"]!
        });
    }

    public class SkillSummary {
        public required string Name { get; set; }
        public required string Attribute { get; set; }
        
        public required string Description { get; set; }
    }
}