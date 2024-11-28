namespace MattEland.BasementsAndBasilisks;

public class BasiliskConfig
{
    public string AzureOpenAiKey { get; set; }
    public string AzureOpenAiEndpoint { get; set; }
    public string AzureOpenAiDeploymentName { get; set; }
    public string AdventuresTableName { get; set; } = "adventures";
    public string AzureStorageTableConnectionString { get; set; }
}