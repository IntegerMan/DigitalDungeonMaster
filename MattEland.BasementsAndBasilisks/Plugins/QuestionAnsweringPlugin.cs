using System.ComponentModel;
using MattEland.BasementsAndBasilisks.Services;
using Microsoft.SemanticKernel;

public class QuestionAnsweringPlugin
{
    private readonly RandomService _rand;

    public QuestionAnsweringPlugin(RandomService rand)
    {
        _rand = rand;
    }

    [KernelFunction("GetAnswer")]
    [Description("Gets a yes, no, or maybe answer to a question from the player.")]
    [return: Description("An answer for the player's question")]
    public string GetAnswer(string question)
    {
        Console.WriteLine("GetAnswer Called");

        int roll = _rand.RollD20();

        return roll switch
        {
            <= 2 => "No, and",
            <= 7 => "No",
            <= 9 => "No, but",
            10 => "Maybe? (skill check or reroll)",
            <= 12 => "Yes, but",
            <= 18 => "Yes",
            _ => "Yes, and"
        };
    }
}