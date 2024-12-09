using MattEland.DigitalDungeonMaster.Services;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
public class StandardPromptsPlugin : GamePlugin
{
    private readonly IChatCompletionService _chatService;
    private readonly ILogger<StandardPromptsPlugin> _logger;

    public StandardPromptsPlugin(RequestContextService context, IChatCompletionService chatService, ILogger<StandardPromptsPlugin> logger) 
        : base(context)
    {
        _chatService = chatService;
        _logger = logger;
    }

    [KernelFunction("EditMessage")]
    [Description("Takes a message intended for the player and improves its quality")]
    public async Task<string?> EditMessage(string input)
    {
        Context.LogPluginCall(input);
        
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

        ChatMessageContent result = await _chatService.GetChatMessageContentAsync(prompt);

        _logger.LogDebug("Edited Message {Input} to {Output}", input, result.Content);

        return result.Content;
    }
}