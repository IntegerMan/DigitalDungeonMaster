using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster;

public interface IChatAgent
{
    string Name { get; }
    Task<ChatResult> ChatAsync(ChatRequest request);
    Task<ChatResult> InitializeAsync(IServiceProvider services);
}