using System.Diagnostics;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.WebAPI.Services;

public class TelemetryEnabledChatCompletionService : IChatCompletionService
{
    private readonly ActivitySource _activitySource;

    public TelemetryEnabledChatCompletionService(IChatCompletionService innerService)
    {
        InnerService = innerService;
        
        _activitySource = new ActivitySource(GetType().Assembly.FullName ?? GetType().Assembly.GetName().Name ?? GetType().FullName ?? "Unknown");
    }

    public IChatCompletionService InnerService { get; set; }

    public IReadOnlyDictionary<string, object?> Attributes => InnerService.Attributes;

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null, 
        CancellationToken cancellationToken = new())
    {
        Activity? activity = _activitySource.StartActivity();

        AddTraces(chatHistory, executionSettings, activity);

        return InnerService.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken).ContinueWith(
            r =>
            {
                try
                {
                    activity?.AddTag("Status", r.Status.ToString());

                    if (r.IsFaulted)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error);
                    }

                    if (r.Exception != null)
                    {
                        activity?.AddException(r.Exception);
                    }

                    if (r.Result is { Count: > 0 })
                    {
                        int index = 1;
                        foreach (var message in r.Result)
                        {
                            activity?.AddTag("Result" + index++, message.Content);
                        }
                    }

                    return r.Result;
                }
                finally
                {
                    activity?.Dispose();
                }
            }, cancellationToken);
    }

    private static void AddTraces(ChatHistory chatHistory, PromptExecutionSettings? executionSettings, Activity? activity)
    {
        // Include chat history
        int index = 1;
        foreach (var message in chatHistory)
        {
            activity?.AddTag($"Message-{index++} ({message.Role})", message.Content);
        }
        
        if (executionSettings != null)
        {
            activity?.AddTag("Model ID", executionSettings.ModelId);
            if (executionSettings.ExtensionData != null)
            {
                foreach (var keyValuePair in executionSettings.ExtensionData)
                {
                    activity?.AddTag($"ExtensionData:{keyValuePair.Key}", keyValuePair.Value);
                }
            }
        }
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null, Kernel? kernel = null,
        CancellationToken cancellationToken = new())
    {
        using Activity? activity = _activitySource.StartActivity();

        AddTraces(chatHistory, executionSettings, activity);
        
        return InnerService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
    }
}