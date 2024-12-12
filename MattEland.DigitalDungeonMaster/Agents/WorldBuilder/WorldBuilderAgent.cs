namespace MattEland.DigitalDungeonMaster.Agents.WorldBuilder;

public sealed class WorldBuilderAgent
{
    private readonly Kernel _kernel;
    private readonly ILogger<WorldBuilderAgent> _logger;

    public WorldBuilderAgent(
        Kernel kernel,
        ILoggerFactory logFactory)
    {
        _kernel = kernel;
        _logger = logFactory.CreateLogger<WorldBuilderAgent>();
    }
    
    public string AgentName => "World Builder";
}