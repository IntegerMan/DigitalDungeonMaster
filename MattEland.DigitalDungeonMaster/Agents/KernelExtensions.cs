using System.ClientModel;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace MattEland.DigitalDungeonMaster.Agents;

internal static class KernelExtensions
{
    internal static async Task<string> SendKernelMessageAsync(this Kernel kernel, IChatRequest request, ILogger logger,
        ChatHistory history, string agentName, string userName)
    {
        logger.LogDebug("{Agent}: {Message}", "User", request.Message);
        history.AddUserMessage(request.Message!.Message!);

        // Set up settings
        OpenAIPromptExecutionSettings settings = new()
        {
            User = userName,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true,
                options: new FunctionChoiceBehaviorOptions
                {
                    AllowConcurrentInvocation = true,
                    AllowParallelCalls = null
                }),
        };

        string? response;
        try
        {
            IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();
            ChatMessageContent result = await chat.GetChatMessageContentAsync(history, settings, kernel);
            history.Add(result);

            response = result.Content ?? "I'm afraid I can't respond to that right now";
            logger.LogDebug("{Agent}: {Message}", agentName, response);
        }
        catch (Exception ex) when (ex is ClientResultException or HttpOperationException)
        {
            response = ex.HandleKernelError(logger);
        }
        
        return response;
    }

    private static string HandleKernelError(this Exception ex, ILogger logger)
    {
        logger.LogError(ex, "{Type} Error: {Message}", ex.GetType().FullName, ex.Message);

        if (ex.Message.Contains("content management", StringComparison.OrdinalIgnoreCase))
        {
            return
                "I'm afraid that message is a bit too spicy for what I'm allowed to process. Can you try something else?";
        }

        if (ex.Message.Contains("429", StringComparison.OrdinalIgnoreCase))
        {
            return "I'm a bit overloaded at the moment. Please wait a minute and try again.";
        }

        if (ex.Message.Contains("server_error", StringComparison.OrdinalIgnoreCase))
        {
            return "There was an error with the large language model that hosts my brain. Please try again later.";
        }

        return "I couldn't handle your request due to an error. Please try again later or report this issue if it persists.";
    }
}