using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.Services;

public class AgentConfigurationService
{
    private readonly IOptionsSnapshot<AgentConfig> _agentOptions;
    private readonly ILogger<AgentConfigurationService> _logger;

    public AgentConfigurationService(IOptionsSnapshot<AgentConfig> agentOptions, ILogger<AgentConfigurationService> logger)
    {
        _agentOptions = agentOptions;
        _logger = logger;
    }
    
    public AgentConfig GetAgentConfiguration(string agentId)
    {
        agentId = agentId.Replace(" ", string.Empty).Trim();
        
        _logger.LogDebug("Loading agent configuration for {AgentId}", agentId);

        return _agentOptions.Get(agentId);
    }
}