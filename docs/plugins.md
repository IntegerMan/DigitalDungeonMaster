# Plugin System Documentation

Digital Dungeon Master uses a sophisticated plugin architecture built on Microsoft Semantic Kernel to provide AI agents with specific capabilities and knowledge domains.

## Plugin Architecture

### Base Plugin Class

All plugins inherit from `PluginBase`, which provides:
- Structured logging capabilities
- Activity tracking for observability
- Consistent error handling patterns

```csharp
public abstract class PluginBase
{
    protected PluginBase(ILogger logger);
    protected ILogger Logger { get; }
    protected Activity? LogActivity(string? message, [CallerMemberName] string? memberName = null);
}
```

### Plugin Registration

Plugins are automatically discovered and registered with their respective AI agents through dependency injection and reflection.

## Available Plugins

### Game Master Agent Plugins

#### Attributes Plugin
**Location**: `MattEland.DigitalDungeonMaster/Agents/GameMaster/Plugins/AttributesPlugin.cs`

Manages character attributes and statistics for different game rulesets.

**Key Functions**:
- `GetAttributesAsync()`: Retrieves available attributes for the current ruleset from Azure Table Storage

**Data Source**: Azure Table Storage (`attributes` table)
- PartitionKey: Ruleset name (e.g., "dnd5e", "pathfinder")  
- RowKey: Attribute name (e.g., "strength", "dexterity")
- Description: Detailed attribute description

**Usage Example**:
```csharp
var attributes = await attributesPlugin.GetAttributesAsync();
// Returns: [{ Name: "Strength", Description: "Physical power and muscle" }, ...]
```

#### Classes Plugin
**Location**: `MattEland.DigitalDungeonMaster/Agents/GameMaster/Plugins/ClassesPlugin.cs`

Handles character classes and their features for different rulesets.

**Key Functions**:
- Provides information about available character classes
- Describes class features and abilities
- Manages class-specific rules and restrictions

**Data Source**: Azure Table Storage (`classes` table)

#### Game Info Plugin
**Location**: `MattEland.DigitalDungeonMaster/Agents/GameMaster/Plugins/GameInfoPlugin.cs`

Provides current game state and player character information.

**Key Functions**:
- `GetPlayerCharacters()`: Retrieves current player character information from blob storage

**Data Source**: Azure Blob Storage (`adventures` container)
- File path: `{userId}_{adventureId}/Players.md`
- Format: Markdown document with character details

#### Image Generation Plugin
**Location**: `MattEland.DigitalDungeonMaster/Agents/GameMaster/Plugins/ImageGenerationPlugin.cs`

Generates AI images using Azure OpenAI DALL-E models.

**Key Functions**:
- `GenerateImageAsync(string description)`: Creates images from text descriptions

**Features**:
- Supports DALL-E 2 and DALL-E 3 models
- Multiple image dimensions (1792x1024, 1024x1024, 1024x1792)
- Automatic image download and local storage
- Base64 encoding for console display

**Configuration Requirements**:
- Azure OpenAI DALL-E deployment
- Image generation deployment name in configuration

#### Location Plugin
**Location**: `MattEland.DigitalDungeonMaster/Agents/GameMaster/Plugins/LocationPlugin.cs`

Manages game locations and environmental details.

**Key Functions**:
- Provides location descriptions and features
- Manages environmental hazards and opportunities
- Tracks location-specific NPCs and objects

**Data Source**: Azure Table Storage integration

#### Session History Plugin
**Location**: `MattEland.DigitalDungeonMaster/Agents/GameMaster/Plugins/SessionHistoryPlugin.cs`

Tracks and manages game session progress and history.

**Key Functions**:
- Maintains session state across gameplay
- Stores important decisions and outcomes
- Provides context for ongoing narratives

**Data Source**: Azure Blob Storage for session files

#### Skills Plugin
**Location**: `MattEland.DigitalDungeonMaster/Agents/GameMaster/Plugins/SkillsPlugin.cs`

Handles character skills and skill check mechanics.

**Key Functions**:
- Provides available skills for the current ruleset
- Describes skill applications and difficulty classes
- Manages skill check resolution

**Data Source**: Azure Table Storage (`skills` table)

#### Standard Prompts Plugin
**Location**: `MattEland.DigitalDungeonMaster/Agents/GameMaster/Plugins/StandardPromptsPlugin.cs`

Provides common game prompts and standardized responses.

**Key Functions**:
- Supplies consistent prompts for common situations
- Manages template responses for recurring scenarios
- Ensures game tone and style consistency

#### Storyteller Plugin
**Location**: `MattEland.DigitalDungeonMaster/Agents/GameMaster/Plugins/StorytellerPlugin.cs`

Specialized narrative generation and story management capabilities.

**Key Functions**:
- Generates engaging narrative descriptions
- Maintains story pacing and tension
- Creates memorable NPCs and dialogue

### World Builder Agent Plugins

#### Setting Creation Plugin
**Location**: `MattEland.DigitalDungeonMaster/Agents/WorldBuilder/Plugins/SettingCreationPlugin.cs`

Creates and manages game worlds and campaign settings.

**Key Functions**:
- Generates cohesive world settings
- Creates geographical features and civilizations
- Establishes world history and lore
- Manages political structures and conflicts

**Data Source**: Azure Table Storage for world configuration data

## Plugin Development Guidelines

### Creating a New Plugin

1. **Inherit from PluginBase**:
```csharp
public class MyPlugin : PluginBase
{
    public MyPlugin(ILogger<MyPlugin> logger) : base(logger)
    {
    }
}
```

2. **Add Function Attributes**:
```csharp
[KernelFunction("FunctionName")]
[Description("Clear description of what this function does")]
public async Task<string> MyFunctionAsync(string parameter)
{
    using var activity = LogActivity($"Parameter: {parameter}");
    // Implementation
    return result;
}
```

3. **Follow Naming Conventions**:
- Plugin class: `{Purpose}Plugin`
- Function names: Descriptive and action-oriented
- Parameter names: Clear and concise

4. **Implement Proper Logging**:
```csharp
using var activity = LogActivity($"Input: {input}");
Logger.LogInformation("Processing {Input}", input);
activity?.AddTag("Result", result);
```

### Best Practices

#### Error Handling
- Always handle exceptions gracefully
- Provide meaningful error messages
- Log errors appropriately
- Return sensible defaults when possible

#### Performance
- Use async/await for I/O operations
- Implement caching for frequently accessed data
- Minimize Azure service calls
- Consider token limits for AI operations

#### Data Access
- Use dependency injection for services
- Abstract data access through interfaces
- Handle connection failures gracefully
- Implement retry logic for transient failures

#### Documentation
- Provide clear function descriptions
- Document parameter requirements
- Include usage examples
- Maintain API documentation

## Plugin Dependencies

### Service Dependencies

Most plugins depend on these core services:

- **IRecordStorageService**: Azure Table Storage access
- **IFileStorageService**: Azure Blob Storage access  
- **RequestContextService**: Current game context
- **ILogger**: Structured logging

### Configuration Dependencies

Plugins may require configuration for:

- Azure service connection strings
- AI model deployment names
- Storage container names
- Feature flags and settings

## Testing Plugins

### Unit Testing
- Mock external dependencies
- Test core logic in isolation
- Verify error handling scenarios
- Check logging and activity tracking

### Integration Testing
- Test with real Azure services in development
- Verify end-to-end plugin functionality
- Test agent integration scenarios
- Validate performance characteristics

### Example Test Structure
```csharp
[Test]
public async Task GetAttributesAsync_ReturnsAttributes_ForValidRuleset()
{
    // Arrange
    var mockStorage = new Mock<IRecordStorageService>();
    var plugin = new AttributesPlugin(mockStorage.Object, context, logger);
    
    // Act
    var result = await plugin.GetAttributesAsync();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.IsTrue(result.Any());
}
```

## Plugin Security Considerations

### Data Validation
- Validate all input parameters
- Sanitize user-provided content
- Check for injection attacks
- Validate file paths and names

### Access Control
- Verify user permissions
- Implement rate limiting
- Audit sensitive operations
- Protect against data exfiltration

### Azure Security
- Use managed identities when possible
- Implement least-privilege access
- Monitor for unusual activity
- Encrypt sensitive data

## Extending the Plugin System

### Adding New Plugin Types

To support new types of plugins:

1. Create appropriate agent classes
2. Register plugins with dependency injection
3. Configure plugin discovery mechanisms
4. Update documentation and examples

### Plugin Composition

Plugins can be composed to create more complex behaviors:

- Chain multiple plugins together
- Share data between related plugins
- Implement plugin communication patterns
- Create plugin hierarchies

### Future Enhancements

Planned improvements to the plugin system:

- Hot-swappable plugin loading
- Plugin marketplace and distribution
- Visual plugin composition tools
- Enhanced debugging and profiling
- Cross-platform plugin support

## Troubleshooting

### Common Issues

1. **Plugin Not Loading**: Check dependency injection registration
2. **Function Not Called**: Verify function attributes and descriptions
3. **Azure Connection Errors**: Check configuration and credentials
4. **Performance Issues**: Review logging and add performance monitoring

### Debugging Tips

- Use the Aspire dashboard to monitor plugin activity
- Enable detailed logging for plugin operations
- Test plugins in isolation before integration
- Monitor Azure service usage and costs

For more information on specific plugins or to contribute new plugins, please refer to the main [README.md](../README.md) and [Development Setup Guide](development-setup.md).