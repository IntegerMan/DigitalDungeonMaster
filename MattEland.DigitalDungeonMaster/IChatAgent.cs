using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster;

public interface IChatAgent<in TRequest> where TRequest : IChatRequest
{
    string Name { get; }
    Task<IChatResult> ChatAsync(TRequest request, string username);
    void Initialize(IServiceProvider services, AgentConfig config);
}