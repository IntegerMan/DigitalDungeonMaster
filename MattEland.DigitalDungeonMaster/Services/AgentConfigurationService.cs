using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.Services;

public class AgentConfigurationService
{
    private readonly IOptionsSnapshot<AgentConfig> _agentOptions;
    private readonly ILogger<AgentConfigurationService> _logger;
    private readonly IConfiguration _config;

    public AgentConfigurationService(IOptionsSnapshot<AgentConfig> agentOptions, ILogger<AgentConfigurationService> logger, IConfiguration config)
    {
        _agentOptions = agentOptions;
        _logger = logger;
        _config = config;
    }
    
    public AgentConfig GetAgentConfiguration(string agentId)
    {
        agentId = agentId.Replace(" ", string.Empty).Trim();
        
        _logger.LogDebug("Loading agent configuration for {AgentId}", agentId);

        AgentConfig agentConfig = _agentOptions.Get(agentId);
        
        if (string.IsNullOrWhiteSpace(agentConfig.Name))
        {
            _logger.LogError("No agent configuration found for {AgentId}", agentId);
            throw new ArgumentOutOfRangeException(nameof(agentId), $"Could not find an agent configuration for {agentId}");
        }
        
        return agentConfig;
    }

    public IEnumerable<string> GetAvailableAgents()
    {
        // Get all agent configurations under the "Agents" section
        IConfigurationSection agentsSection = _config.GetSection("Agents");
        
        // For each one, load its agent configuration
        IEnumerable<AgentConfig> configs = agentsSection.GetChildren().Select(section => section.Get<AgentConfig>())!;
        
        // Return the names of the agents
        return configs.Where(a => a.IsAvailableInGame).Select(config => config.Name);
    }
}