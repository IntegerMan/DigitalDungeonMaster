using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.Services;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "QuestionAnswering")]
public class QuestionAnsweringPlugin
{
    private readonly RandomService _rand;
    private readonly RequestContextService _context;

    public QuestionAnsweringPlugin(RandomService rand, RequestContextService context)
    {
        _rand = rand;
        _context = context;
    }

    [KernelFunction("GetAnswer")]
    [Description("Gets a yes, no, or maybe answer to a question from the player.")]
    [return: Description("An answer for the player's question")]
    public string GetAnswer(string question)
    {
        _context.LogPluginCall(metadata: question);

        int roll = _rand.RollD20();

        string answer = roll switch
        {
            <= 2 => "No, and",
            <= 7 => "No",
            <= 9 => "No, but",
            10 => "Maybe? (perform a skill check or ask a question to clarify)",
            <= 12 => "Yes, but",
            <= 18 => "Yes",
            _ => "Yes, and"
        };
        
        _context.AddBlock(new TextResourceBlock($"{nameof(GetAnswer)} Result",$"{answer} (d20 roll: {roll})"));
        
        return answer;
    }
}