using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Storyteller Plugin is responsible for managing the game's story and non-player-facing notes.")]
public class StorytellerPlugin : GamePlugin
{
    private readonly RandomService _rand;
    private readonly ILogger<StorytellerPlugin> _logger;
    private readonly List<string> _notes = new();
    
    public StorytellerPlugin(RequestContextService context, RandomService rand, ILogger<StorytellerPlugin> logger) : base(context)
    {
        _rand = rand;
        _logger = logger;
    }

    [KernelFunction(nameof(this.AddPrivateNote)), 
     Description("Adds a private note to the Storyteller's notes. This is a way of keeping information handy for the DM that the player can't see.")]
    public string AddPrivateNote(string note)
    {
        Context.LogPluginCall(note);
        
        return $"Private note added. Don't tell the player about the contents of this note. You can check notes in the future by calling {nameof(GetNotes)}.";
    }
    
    [KernelFunction(nameof(this.GetNotes)), 
     Description("Gets all private story notes you've left for yourself. This can be a good way of finding information secret from the player.")]
    public IEnumerable<string> GetNotes()
    {
        Context.LogPluginCall($"{_notes.Count} Current Notes");
        
        _logger.LogTrace("Current Notes: {Notes}", _notes);
        
        return _notes;
    }
    
    [KernelFunction("GetAnswer")]
    [Description("If you are uncertain of something that has a yes or no question, this will give you a yes, no, or maybe answer")]
    public string GetAnswer(string question)
    {
        Context.LogPluginCall(metadata: question);

        int roll = _rand.RollD20();

        string answer = roll switch
        {
            <= 2 => "No, and it's even worse than that",
            <= 7 => "No",
            <= 9 => "No, but there's a positive side to it",
            10 => "Maybe? (ask for a relevant skill check or ask a question to clarify)",
            <= 12 => "Yes, but there's a negative side as well",
            <= 18 => "Yes",
            _ => "Yes, and it's even better than that"
        };
        
        _logger.LogInformation("Oracle called with {Question}, rolled a {Roll}, and answered {Answer}", question, roll, answer);
        
        return answer;
    }
}