# Azure Setup Guide

This guide provides detailed instructions for setting up the Azure services required for Digital Dungeon Master.

## Prerequisites

- Azure subscription with appropriate permissions
- Azure CLI installed (optional but recommended)
- PowerShell or bash terminal

## Azure OpenAI Service Setup

### 1. Create Azure OpenAI Resource

```bash
# Using Azure CLI
az cognitiveservices account create \
    --name "ddm-openai" \
    --resource-group "your-resource-group" \
    --location "eastus" \
    --kind "OpenAI" \
    --sku "S0"
```

Or create via Azure Portal:
1. Navigate to Azure Portal > Create a resource
2. Search for "Azure OpenAI"
3. Fill in the required details:
   - **Subscription**: Your Azure subscription
   - **Resource Group**: Create new or use existing
   - **Region**: Choose a region with GPT-4 availability (e.g., East US, West Europe)
   - **Name**: Unique name for your OpenAI resource
   - **Pricing Tier**: Standard (S0)

### 2. Deploy Required Models

Navigate to Azure OpenAI Studio or use the following commands:

#### Deploy GPT-4 Model (Required)
```bash
az cognitiveservices account deployment create \
    --resource-group "your-resource-group" \
    --account-name "ddm-openai" \
    --deployment-name "gpt-4" \
    --model-name "gpt-4" \
    --model-version "0613" \
    --model-format "OpenAI" \
    --scale-type "Standard"
```

#### Deploy DALL-E Model (Optional)
```bash
az cognitiveservices account deployment create \
    --resource-group "your-resource-group" \
    --account-name "ddm-openai" \
    --deployment-name "dall-e-3" \
    --model-name "dall-e-3" \
    --model-version "3.0" \
    --model-format "OpenAI" \
    --scale-type "Standard"
```

#### Deploy Embedding Model (Optional)
```bash
az cognitiveservices account deployment create \
    --resource-group "your-resource-group" \
    --account-name "ddm-openai" \
    --deployment-name "text-embedding-ada-002" \
    --model-name "text-embedding-ada-002" \
    --model-version "2" \
    --model-format "OpenAI" \
    --scale-type "Standard"
```

### 3. Get Connection Information

```bash
# Get the endpoint
az cognitiveservices account show \
    --resource-group "your-resource-group" \
    --name "ddm-openai" \
    --query "properties.endpoint" \
    --output tsv

# Get the API key
az cognitiveservices account keys list \
    --resource-group "your-resource-group" \
    --name "ddm-openai" \
    --query "key1" \
    --output tsv
```

## Azure Storage Account Setup

### 1. Create Storage Account

```bash
az storage account create \
    --name "ddmstorage" \
    --resource-group "your-resource-group" \
    --location "eastus" \
    --sku "Standard_LRS" \
    --kind "StorageV2"
```

### 2. Get Connection String

```bash
az storage account show-connection-string \
    --name "ddmstorage" \
    --resource-group "your-resource-group" \
    --query "connectionString" \
    --output tsv
```

### 3. Create Required Containers

```bash
# Set connection string as environment variable
export AZURE_STORAGE_CONNECTION_STRING="your-connection-string"

# Create blob containers
az storage container create --name "adventures"
az storage container create --name "assets"
az storage container create --name "images"
```

## Required Azure Table Storage Tables

The application expects the following tables to exist in your Azure Storage account:

### Core Tables

1. **attributes** - Character attributes by ruleset
   - PartitionKey: Ruleset name (e.g., "dnd5e", "pathfinder")
   - RowKey: Attribute name (e.g., "strength", "dexterity")
   - Description: Attribute description

2. **classes** - Character classes by ruleset
   - PartitionKey: Ruleset name
   - RowKey: Class name (e.g., "fighter", "wizard")
   - Description: Class description and features

3. **skills** - Available skills by ruleset
   - PartitionKey: Ruleset name
   - RowKey: Skill name (e.g., "athletics", "arcana")
   - Description: Skill description and uses

4. **users** - User account information
   - PartitionKey: "users"
   - RowKey: User identifier
   - Additional user properties

5. **adventures** - Campaign and adventure data
   - PartitionKey: User identifier
   - RowKey: Adventure identifier
   - Adventure metadata and state

6. **settings** - Game world settings
   - PartitionKey: Setting type
   - RowKey: Setting identifier
   - Setting configuration data

### Creating Tables

Tables will be automatically created by the application when first accessed, but you can create them manually:

```bash
# Create tables using Azure CLI
az storage table create --name "attributes"
az storage table create --name "classes"
az storage table create --name "skills"
az storage table create --name "users"
az storage table create --name "adventures"
az storage table create --name "settings"
```

## Resource Naming Conventions

For consistency and easier management, consider using these naming conventions:

- **Resource Group**: `rg-ddm-[environment]` (e.g., `rg-ddm-dev`, `rg-ddm-prod`)
- **OpenAI Service**: `ddm-openai-[environment]`
- **Storage Account**: `ddmstorage[environment]` (lowercase, no hyphens)
- **App Service Plan**: `asp-ddm-[environment]`

## Security Considerations

1. **Access Control**: Use Azure RBAC to limit access to resources
2. **Key Management**: Consider using Azure Key Vault for secret management
3. **Network Security**: Configure firewall rules and private endpoints as needed
4. **Monitoring**: Enable Azure Monitor and Application Insights for observability

## Cost Optimization

1. **GPT-4 Usage**: Monitor token usage as GPT-4 can be expensive
2. **Storage Tiers**: Use appropriate storage tiers for different types of data
3. **Auto-scaling**: Configure auto-scaling for compute resources
4. **Cleanup**: Remove unused deployments and resources

## Troubleshooting

### Common Issues

1. **Model Not Available**: Ensure the chosen region supports GPT-4 models
2. **Quota Limits**: Check and request quota increases if needed
3. **Connection Issues**: Verify firewall settings and network connectivity
4. **Authentication**: Ensure API keys and connection strings are correct

### Useful Commands

```bash
# Check OpenAI service status
az cognitiveservices account show --name "ddm-openai" --resource-group "your-rg"

# List deployed models
az cognitiveservices account deployment list --name "ddm-openai" --resource-group "your-rg"

# Test storage connectivity
az storage blob list --container-name "adventures" --output table
```

## Next Steps

Once Azure resources are configured:

1. Update your application configuration with the connection details
2. Run the application to verify connectivity
3. Initialize any required seed data
4. Set up monitoring and alerts
5. Configure backup and disaster recovery

For application configuration, see [Development Setup Guide](development-setup.md).