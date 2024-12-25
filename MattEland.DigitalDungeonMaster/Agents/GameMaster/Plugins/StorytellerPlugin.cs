using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("The Storyteller Plugin is responsible for managing the game's story and non-player-facing notes.")]
public class StorytellerPlugin : PluginBase
{
    private readonly RandomService _rand;
    private readonly List<string> _notes = new();

    public StorytellerPlugin(RandomService rand, ILogger<StorytellerPlugin> logger)
        : base(logger)
    {
        _rand = rand;
    }

    [KernelFunction(nameof(this.AddPrivateNote)),
     Description("Adds a private note to the Storyteller's notes. This is a way of keeping information handy for the DM that the player can't see.")]
    public string AddPrivateNote(string note)
    {
        using Activity? activity = LogActivity($"Adding private note: {note}");

        // TODO: Actually add this somewhere persistent
        _notes.Add(note);

        return "Private note added. Don't tell the player about the contents of this note.";
    }

    [KernelFunction(nameof(this.GetNotes)),
     Description("Gets all private story notes you've left for yourself. This can be a good way of finding information secret from the player.")]
    public IEnumerable<string> GetNotes()
    {
        using Activity? activity = LogActivity("Getting private notes");
        Logger.LogTrace("Current Notes: {Notes}", _notes);

        activity?.AddTag("NoteCount", _notes.Count);
        
        int index = 1;
        foreach (var note in _notes)
        {
            activity?.AddTag($"Note-{index++}", note);
        }

        return _notes;
    }

    [KernelFunction("GetAnswer")]
    [Description(
        "If you are uncertain of something that has a yes or no question, this will give you a yes, no, or maybe answer")]
    public string GetAnswer(string question)
    {
        using Activity? activity = LogActivity($"Question: {question}");

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

        Logger.LogInformation("Oracle called with {Question}, rolled a {Roll}, and answered {Answer}", question, roll,
            answer);

        activity?.AddTag("Roll", roll);
        activity?.AddTag("Answer", answer);

        return answer;
    }
}