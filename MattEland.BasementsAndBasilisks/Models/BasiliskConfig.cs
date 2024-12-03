namespace MattEland.BasementsAndBasilisks;

public class BasiliskConfig
{
    public string AzureOpenAiKey { get; set; } = string.Empty;
    public string AzureOpenAiEndpoint { get; set; } = string.Empty;
    public string AzureOpenAiChatDeploymentName { get; set; } = string.Empty;
    public string AzureOpenAiEmbeddingDeploymentName { get; set; } = string.Empty;
    public string AzureOpenAiImageDeploymentName { get; set; } = string.Empty;
    public string AzureStorageConnectionString { get; set; } = string.Empty;
}