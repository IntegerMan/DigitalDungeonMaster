using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.Services;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.BasementsAndBasilisks.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[BasiliskPlugin(PluginName = "StandardPrompts")]
public class StandardPromptsPlugin : BasiliskPlugin
{
    private readonly RequestContextService _context;

    public StandardPromptsPlugin(RequestContextService context)
    {
        _context = context;
        //_chat = chat;
    }

    [KernelFunction("EditMessage")]
    [Description("Takes a message intended for the player and improves its quality")]
    [return: Description("The improved message")]
    public async Task<string?> EditMessage(string input)
    {
        _context.LogPluginCall(input);
        string prompt = $"""
                         You are an editor designed to polish messages intended for the player.
                         Messages are from a dungeon master (DM) and intended to be read by a player
                         in a role playing game. Make sure that responses are constrained in length
                         and do not use lists. In places where the DM assumes actions the player didn't
                         intend to take, remove these from the response in order to preserve player agency.
                         A good response will tell the player what happens, direct them to take necessary
                         actions, roll dice, have combat encounters, or make decisions.

                         The message to edit is: {input}
                         """;

        IChatCompletionService chat = Kernel!.GetRequiredService<IChatCompletionService>();
        ChatMessageContent result = await chat.GetChatMessageContentAsync(prompt);

        _context.AddBlock(new DiagnosticBlock
        {
            Header = nameof(EditMessage),
            Metadata = result.Content
        });

        return result.Content;
    }
}