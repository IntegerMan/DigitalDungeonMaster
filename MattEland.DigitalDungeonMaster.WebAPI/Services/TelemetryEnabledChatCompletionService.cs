using System.Diagnostics;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0001

namespace MattEland.DigitalDungeonMaster.WebAPI.Services;

public class TelemetryEnabledChatCompletionService : IChatCompletionService
{
    private readonly ActivitySource _activitySource;
    private readonly IChatCompletionService _innerService;

    public TelemetryEnabledChatCompletionService(IChatCompletionService innerService)
    {
        _innerService = innerService;

        _activitySource = new ActivitySource(GetType().Assembly.FullName ??
                                             GetType().Assembly.GetName().Name ?? GetType().FullName ?? "Unknown");
    }

    public IReadOnlyDictionary<string, object?> Attributes => _innerService.Attributes;

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = new())
    {
        Activity? activity = _activitySource.StartActivity();

        AddTraces(chatHistory, executionSettings, kernel, activity);

        return _innerService.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken)
            .ContinueWith(
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
                                activity?.AddTag($"Result-{index++}", message.Content);
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

    private static void AddTraces(ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings,
        Kernel? kernel,
        Activity? activity)
    {
        // Include chat history
        int index = 1;
        foreach (var message in chatHistory)
        {
            activity?.AddTag($"Message-{index++} ({message.Role})", message.Content);
        }

        AddExecutionSettingsTraces(executionSettings, activity);
        AddKernelTraces(kernel, activity);
    }

    internal static void AddKernelTraces(Kernel? kernel, Activity? activity)
    {
        if (kernel != null)
        {
            foreach (var plugin in kernel.Plugins)
            {
                activity?.AddTag($"Kernel Plugin {plugin.Name}", plugin.Description);

                foreach (var func in plugin.GetFunctionsMetadata())
                {
                    activity?.AddTag($"Kernel Function {plugin.Name}-{func.Name}", func.Description);
                }
            }

            foreach (var data in kernel.Data)
            {
                activity?.AddTag($"Kernel Data {data.Key}", data.Value);
            }

            int index = 1;
            foreach (var filter in kernel.FunctionInvocationFilters)
            {
                activity?.AddTag($"Kernel Function Filter {index++}", filter.GetType().FullName);
            }

            index = 1;
            foreach (var filter in kernel.PromptRenderFilters)
            {
                activity?.AddTag($"Kernel Prompt Render Filter {index++}", filter.GetType().FullName);
            }
            
            index = 1;
            foreach (var filter in kernel.AutoFunctionInvocationFilters)
            {
                activity?.AddTag($"Kernel Prompt Response Filter {index++}", filter.GetType().FullName);
            }
        }
    }

    internal static void AddExecutionSettingsTraces(PromptExecutionSettings? executionSettings, Activity? activity)
        {
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

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null, Kernel? kernel = null,
            CancellationToken cancellationToken = new())
        {
            using Activity? activity = _activitySource.StartActivity();

            AddTraces(chatHistory, executionSettings, kernel, activity);

            return _innerService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, kernel,
                cancellationToken);
        }
    }