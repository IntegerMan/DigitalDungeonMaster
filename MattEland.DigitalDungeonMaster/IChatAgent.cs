using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster;

public interface IChatAgent<in TRequest, TResult> 
    where TRequest : IChatRequest
    where TResult : IChatResult
{
    string Name { get; }
    Task<TResult> ChatAsync(TRequest request, string username, CancellationToken token = default);
    void Initialize(IServiceProvider services, AgentConfig config);
}