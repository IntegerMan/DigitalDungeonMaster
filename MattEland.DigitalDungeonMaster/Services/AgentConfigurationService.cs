using MattEland.DigitalDungeonMaster.Models;
using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.Services;

public class AgentConfigurationService
{
    private readonly AgentConfig _agentConfig;

    public AgentConfigurationService(IOptionsSnapshot<AgentConfig> agentConfig)
    {
        _agentConfig = agentConfig.Value;
    }
    
    public AgentConfig GetAgentConfiguration(string agentId)
    {
        // TODO: Support keyed agent lookup
        
        /* Old code to load from a JSON File in blob storage:
       // Load our resources 
       string key = $"{_context.CurrentUser}_{_context.CurrentAdventureId}/gameconfig.json";
       string? json = await _storage.LoadTextOrDefaultAsync("adventures", key);

       if (string.IsNullOrWhiteSpace(json))
       {
           throw new InvalidOperationException("The configuration for the current game could not be found");
       }
       // Deserialize the JSON into a configuration object
       KernelConfig? kernelConfig = JsonConvert.DeserializeObject<KernelConfig>(json);
       if (kernelConfig is null)
       {
           _logger.Warning("No configuration found for {Key}", key);
           throw new InvalidOperationException("The configuration for the current game could not be loaded");
       }
       
       GameAgentConfig agent = kernelConfig.Agents.FirstOrDefault(a =>
           string.Equals(a.Name, "DM", StringComparison.OrdinalIgnoreCase)) ?? kernelConfig.Agents.First();
           
        */
        
        return _agentConfig;
    }
}