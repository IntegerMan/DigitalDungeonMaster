using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated via Dependency Injection")]
[Description("The Storyteller Plugin is responsible for managing the game's story and non-player-facing notes.")]
public class StorytellerPlugin(
    RandomService rand,
    ILogger<StorytellerPlugin> logger,
    NotesService notesService,
    RequestContextService context)
    : PluginBase(logger)
{
    [KernelFunction(nameof(this.AddPrivateNote)),
     Description(
         "Adds a private note to the Storyteller's notes. This is a way of keeping information handy for the DM that the player can't see.")]
    public async Task<string> AddPrivateNote(string note)
    {
        using Activity? activity = LogActivity($"Adding private note: {note}");

        if (string.IsNullOrWhiteSpace(note))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "No note provided");
            Logger.LogWarning($"No note provided for {nameof(AddPrivateNote)}");
            return "It looks like you didn't provide a note. Please provide a note to add.";
        }

        await notesService.AddPrivateNoteAsync(context.CurrentUser!, context.CurrentAdventure!.RowKey, note);

        return "Private note added. Don't tell the player about the contents of this note.";
    }

    [KernelFunction(nameof(this.GetNotes)),
     Description(
         "Gets all private story notes you've left for yourself. This can be a good way of finding information secret from the player.")]
    public async Task<IEnumerable<string>> GetNotes()
    {
        using Activity? activity = LogActivity("Getting private notes");

        string[] notes = (await notesService.GetPrivateNotesAsync(context.CurrentUser!, context.CurrentAdventure!.RowKey)).ToArray();

        activity?.AddTag("NoteCount", notes.Length);

        int index = 1;
        foreach (var note in notes)
        {
            activity?.AddTag($"Note-{index++}", note);
        }

        return notes;
    }

    [KernelFunction("GetAnswer")] // TODO: This doesn't seem to be getting called. Might need some prompt engineering and experimentation
    [Description(
        "If you are uncertain of something that has a yes or no question, this will give you a yes, no, or maybe answer")]
    public string GetAnswer(string question)
    {
        using Activity? activity = LogActivity($"Question: {question}");

        int roll = rand.RollD20();

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