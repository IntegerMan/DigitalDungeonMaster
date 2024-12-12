using MattEland.DigitalDungeonMaster.Models;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster;

public interface IChatAgent
{
    string Name { get; }
    Task<ChatResult> ChatAsync(ChatRequest request);
    Task<ChatResult> InitializeAsync(IServiceProvider services);
}