using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster;

public abstract class AgentBase : IChatAgent
{
    protected AgentBase(ILogger logger)
    {
        Logger = logger;
    }
    protected ILogger Logger { get; }
    protected void CopyRequestHistory(IChatRequest request, ChatHistory history)
    {
        if (request.History == null) return;
        
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

    public abstract string Name { get; }
    public abstract Task<IChatResult> ChatAsync(IChatRequest request, string username);
    public abstract void Initialize(IServiceProvider services, AgentConfig config);
}