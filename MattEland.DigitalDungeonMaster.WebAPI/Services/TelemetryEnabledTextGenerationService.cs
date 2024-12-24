using System.Diagnostics;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;

namespace MattEland.DigitalDungeonMaster.WebAPI.Services;

public class TelemetryEnabledTextGenerationService : ITextGenerationService
{
    private readonly ITextGenerationService _innerService;
    private readonly ActivitySource _activitySource;

    public TelemetryEnabledTextGenerationService(ITextGenerationService innerService)
    {
        _innerService = innerService;
        
        _activitySource = new ActivitySource(GetType().Assembly.FullName ?? GetType().Assembly.GetName().Name ?? GetType().FullName ?? "Unknown");
    }

    public IReadOnlyDictionary<string, object?> Attributes => _innerService.Attributes;

    public Task<IReadOnlyList<TextContent>> GetTextContentsAsync(string prompt,
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null,
        CancellationToken cancellationToken = new())
    {
        Activity? activity = _activitySource.StartActivity();
        
        activity?.AddTag("Prompt", prompt);
        TelemetryEnabledChatCompletionService.AddExecutionSettingsTraces(executionSettings, activity);
        TelemetryEnabledChatCompletionService.AddKernelTraces(kernel, activity);

        return _innerService.GetTextContentsAsync(prompt, executionSettings, kernel, cancellationToken).ContinueWith(
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
                            activity?.AddTag("Result" + index++, message.Text);
                        }
                    }

                    return r.Result;
                }
                finally
                {
                    activity?.Dispose();
                }
            });
    }

    public IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null, CancellationToken cancellationToken = new())
    {
        using Activity? activity = _activitySource.StartActivity();
        
        activity?.AddTag("Prompt", prompt);
        TelemetryEnabledChatCompletionService.AddExecutionSettingsTraces(executionSettings, activity);
        TelemetryEnabledChatCompletionService.AddKernelTraces(kernel, activity);

        return _innerService.GetStreamingTextContentsAsync(prompt, executionSettings, kernel, cancellationToken);
    }
}