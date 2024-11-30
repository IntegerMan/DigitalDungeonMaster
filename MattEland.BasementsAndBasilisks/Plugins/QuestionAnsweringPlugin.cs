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
    [Description("If you are uncertain of something that has a yes or no question, this will give you a yes, no, or maybe answer")]
    [return: Description("Gets a yes, no, or maybe answer")]
    public string GetAnswer(string question)
    {
        _context.LogPluginCall(metadata: question);

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
        
        _context.AddBlock(new TextResourceBlock($"{nameof(GetAnswer)} Result",$"{answer} (d20 roll: {roll})"));
        
        return answer;
    }
}