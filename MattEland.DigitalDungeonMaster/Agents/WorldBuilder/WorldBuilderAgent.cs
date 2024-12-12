using MattEland.DigitalDungeonMaster.Agents.GameMaster;
using MattEland.DigitalDungeonMaster.Models;

namespace MattEland.DigitalDungeonMaster.Agents.WorldBuilder;

public sealed class WorldBuilderAgent : IChatAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger<WorldBuilderAgent> _logger;

    public WorldBuilderAgent(
        Kernel kernel,
        ILoggerFactory logFactory)
    {
        _kernel = kernel.Clone();
        _logger = logFactory.CreateLogger<WorldBuilderAgent>();
    }
    
    public string Name => "World Builder";
    
    public Task<ChatResult> InitializeAsync(IServiceProvider services)
    {
        throw new NotImplementedException();
    }
    
    public Task<ChatResult> ChatAsync(ChatRequest request)
    {
        throw new NotImplementedException();
    }
}