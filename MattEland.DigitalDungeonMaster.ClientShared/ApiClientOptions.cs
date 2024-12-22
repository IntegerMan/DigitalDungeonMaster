namespace MattEland.DigitalDungeonMaster.ClientShared;

public class ApiClientOptions
{
    /// <summary>
    /// The base URL for the API. This defaults to the deployed version on Azure, preferring HTTPS over HTTP
    /// When running using Aspire, this will be changed to the local API instance's endpoint
    /// </summary>
    public string BaseUrl { get; set; } = "https+http://basilisk.azurewebsites.net";
}