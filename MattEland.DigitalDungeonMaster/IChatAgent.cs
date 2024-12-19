using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster;

public interface IChatAgent
{
    string Name { get; }
    Task<IChatResult> ChatAsync(IChatRequest request, string username);
    void Initialize(IServiceProvider services, AgentConfig config);
}