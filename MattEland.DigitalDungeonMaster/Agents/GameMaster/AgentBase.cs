using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster;

public abstract class AgentBase<TRequest, TResult> : IChatAgent<TRequest, TResult> 
    where TRequest : IChatRequest 
    where TResult : IChatResult
{
    protected AgentBase(ILogger logger)
    {
        Logger = logger;
    }
    protected ILogger Logger { get; }
    protected void CopyRequestHistory(TRequest request, ChatHistory history)
    {
        if (request.History == null) return;
        
        // TODO: This should probably truncate at some point
        
        foreach (var entry in request.History)
        {
            Logger.LogTrace("{Author}: {Message}", entry.Author, entry.Message);

            if (!string.IsNullOrWhiteSpace(entry.Message))
            {
                if (entry.Author == request.User)
                {
                    history.AddUserMessage(entry.Message!);
                }
                else
                {
                    history.AddAssistantMessage(entry.Message!);
                }
            }
        }
    }

    public abstract string Name { get; protected set; }
    public abstract Task<TResult> ChatAsync(TRequest request, string username, CancellationToken token = default);
    public abstract void Initialize(IServiceProvider services, AgentConfig config);
}