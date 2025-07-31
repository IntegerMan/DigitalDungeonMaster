# Development Setup Guide

This guide provides detailed instructions for setting up a local development environment for Digital Dungeon Master.

## Prerequisites

### Required Software

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Visual Studio 2022** (17.8+) or **VS Code** with C# Dev Kit
- **Git** for source control
- **Azure subscription** with required services (see [Azure Setup Guide](azure-setup.md))

### Optional Tools

- **Azure CLI** - For managing Azure resources
- **Azure Storage Explorer** - For browsing storage data
- **Postman** or similar - For API testing
- **Docker Desktop** - If using containerized development

## Project Structure

Understanding the solution structure:

```
DigitalDungeonMaster/
├── MattEland.DigitalDungeonMaster/              # Core business logic & AI agents
├── MattEland.DigitalDungeonMaster.AspireHost/   # .NET Aspire orchestration
├── MattEland.DigitalDungeonMaster.WebAPI/       # REST API service
├── MattEland.DigitalDungeonMaster.ConsoleApp/   # Console client application
├── MattEland.DigitalDungeonMaster.AvaloniaApp/  # Desktop client application
├── MattEland.DigitalDungeonMaster.Services/     # Service interfaces
├── MattEland.DigitalDungeonMaster.Services.Azure/ # Azure service implementations
├── MattEland.DigitalDungeonMaster.Shared/       # Shared models and utilities
├── MattEland.DigitalDungeonMaster.ClientShared/ # Shared client code
├── MattEland.DigitalDungeonMaster.ServiceDefaults/ # Service configuration
├── MattEland.DigitalDungeonMaster.Tests/        # Unit tests
└── docs/                                        # Documentation
```

## Initial Setup

### 1. Clone the Repository

```bash
git clone https://github.com/IntegerMan/DigitalDungeonMaster.git
cd DigitalDungeonMaster
```

### 2. Configure User Secrets

User secrets provide a secure way to store configuration during development:

```bash
# Initialize user secrets for the main projects
dotnet user-secrets init --project MattEland.DigitalDungeonMaster.ConsoleApp
dotnet user-secrets init --project MattEland.DigitalDungeonMaster.WebAPI
dotnet user-secrets init --project MattEland.DigitalDungeonMaster.AvaloniaApp

# Set Azure OpenAI configuration
dotnet user-secrets set "AzureResources:AzureOpenAiKey" "your-azure-openai-key" --project MattEland.DigitalDungeonMaster.ConsoleApp
dotnet user-secrets set "AzureResources:AzureOpenAiEndpoint" "https://your-resource.openai.azure.com/" --project MattEland.DigitalDungeonMaster.ConsoleApp
dotnet user-secrets set "AzureResources:AzureOpenAiChatDeploymentName" "gpt-4" --project MattEland.DigitalDungeonMaster.ConsoleApp
dotnet user-secrets set "AzureResources:AzureOpenAiImageDeploymentName" "dall-e-3" --project MattEland.DigitalDungeonMaster.ConsoleApp
dotnet user-secrets set "AzureResources:AzureStorageConnectionString" "your-storage-connection-string" --project MattEland.DigitalDungeonMaster.ConsoleApp

# Repeat for other projects
dotnet user-secrets set "AzureResources:AzureOpenAiKey" "your-azure-openai-key" --project MattEland.DigitalDungeonMaster.WebAPI
# ... continue for all configuration values and projects
```

### 3. Verify Build

```bash
# Restore dependencies and build the solution
dotnet restore
dotnet build
```

## Running the Application

### Option 1: .NET Aspire (Recommended)

Aspire provides orchestration, service discovery, and monitoring:

```bash
dotnet run --project MattEland.DigitalDungeonMaster.AspireHost
```

This will:
- Start the Web API service
- Start your chosen client application (Console or Avalonia)
- Launch the Aspire dashboard at `http://localhost:15000`
- Provide service discovery and distributed tracing

### Option 2: Individual Projects

#### Run Web API Directly
```bash
dotnet run --project MattEland.DigitalDungeonMaster.WebAPI
```
API will be available at `https://localhost:7001`

#### Run Console App
```bash
dotnet run --project MattEland.DigitalDungeonMaster.ConsoleApp
```

#### Run Avalonia Desktop App
```bash
dotnet run --project MattEland.DigitalDungeonMaster.AvaloniaApp
```

## Development Workflow

### IDE Setup

#### Visual Studio 2022
1. Open `MattEland.DigitalDungeonMaster.sln`
2. Set `MattEland.DigitalDungeonMaster.AspireHost` as startup project
3. Configure multiple startup projects if needed:
   - Right-click solution → Properties → Multiple Startup Projects
   - Set desired projects to "Start"

#### VS Code
1. Open the repository folder
2. Install recommended extensions:
   - C# Dev Kit
   - .NET Aspire extension
3. Use `Ctrl+Shift+P` → ".NET: Generate Assets for Build and Debug"

### Debugging

#### Full-Stack Debugging with Aspire
1. Set breakpoints in any project
2. Start debugging with F5
3. Use the Aspire dashboard to monitor services
4. View distributed traces across services

#### Individual Project Debugging
1. Set the target project as startup project
2. Configure launch settings as needed
3. Use standard debugging tools

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test MattEland.DigitalDungeonMaster.Tests
```

## Configuration Management

### appsettings.json Structure

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AzureResources": {
    "AzureOpenAiChatDeploymentName": "gpt-4",
    "AzureOpenAiImageDeploymentName": "dall-e-3",
    "AzureOpenAiEmbeddingDeploymentName": "text-embedding-ada-002"
  }
}
```

### Environment-Specific Configuration

- **Development**: Use user secrets for sensitive data
- **Staging/Production**: Use Azure Key Vault or environment variables
- **CI/CD**: Use Azure DevOps variables or GitHub Actions secrets

## Plugin Development

### Creating a New Plugin

1. Create a new class inheriting from `PluginBase`
2. Add the plugin to the appropriate agent
3. Use `[KernelFunction]` and `[Description]` attributes
4. Register the plugin in dependency injection

Example:
```csharp
[Description("Example plugin for demonstration")]
public class ExamplePlugin : PluginBase
{
    public ExamplePlugin(ILogger<ExamplePlugin> logger) : base(logger)
    {
    }

    [KernelFunction("DoSomething")]
    [Description("Does something useful")]
    public async Task<string> DoSomethingAsync(string input)
    {
        using var activity = LogActivity($"Input: {input}");
        // Plugin logic here
        return "Result";
    }
}
```

### Plugin Guidelines

- Always inherit from `PluginBase`
- Use appropriate logging and activity tracking
- Keep functions focused and single-purpose
- Provide clear descriptions for AI agent usage
- Handle errors gracefully

## Troubleshooting

### Common Issues

#### Build Errors
- **Issue**: Missing .NET 9.0 SDK
- **Solution**: Install .NET 9.0 SDK from Microsoft

#### Runtime Errors
- **Issue**: Azure connection failures
- **Solution**: Verify Azure configuration and connectivity

#### Missing Dependencies
- **Issue**: Package restore failures
- **Solution**: Clear package cache: `dotnet nuget locals all --clear`

### Debugging Tips

1. **Use Aspire Dashboard**: Monitor service health and traces
2. **Check Logs**: Use structured logging throughout the application
3. **Verify Configuration**: Ensure all required settings are configured
4. **Test Azure Connectivity**: Use Azure CLI to verify resource access

### Performance Considerations

1. **AI Model Usage**: Monitor token consumption and costs
2. **Storage Access**: Optimize table and blob storage queries
3. **Memory Usage**: Profile memory usage with dotMemory or similar tools
4. **Async Patterns**: Ensure proper async/await usage throughout

## Contributing Guidelines

### Code Style

- Follow .NET coding conventions
- Use nullable reference types
- Implement proper logging and error handling
- Write XML documentation for public APIs

### Pull Request Process

1. Fork the repository
2. Create a feature branch
3. Implement changes with tests
4. Ensure all tests pass
5. Submit a pull request with clear description

### Testing

- Write unit tests for new functionality
- Use integration tests for external dependencies
- Mock Azure services for testing
- Maintain good test coverage

## Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Azure OpenAI Documentation](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/)
- [Avalonia Documentation](https://docs.avaloniaui.net/)

## Next Steps

1. Complete Azure setup following the [Azure Setup Guide](azure-setup.md)
2. Run the application and verify functionality
3. Explore the codebase and plugin system
4. Consider contributing new features or improvements