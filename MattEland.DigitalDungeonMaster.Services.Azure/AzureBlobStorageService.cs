using System.Text;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MattEland.DigitalDungeonMaster.Services.Azure;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly BlobServiceClient _blobClient;

    public AzureBlobStorageService(IOptionsSnapshot<AzureResourceConfig> config, ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        _blobClient = new BlobServiceClient(config.Value.AzureStorageConnectionString);
    }

    private static IEnumerable<TOutput> MapResults<TOutput>(Func<IDictionary<string, object?>, TOutput> mapper, List<TableEntity> results)
    {
        return results.Select(e =>
        {
            IDictionary<string, object?> values = new Dictionary<string, object?>();
            foreach (var property in e)
            {
                values[property.Key] = property.Value;
            }

            return mapper(values);
        });
    }

    public async Task<string> LoadTextAsync(string container, string path)
    {
        _logger.LogDebug("Loading Text Resource: {Container}, {Path}", container, path);

        BlobContainerClient containerClient = _blobClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);
        
        return await ReadBlobDataAsync(blobClient);
    }
    
    public async Task<string?> LoadTextOrDefaultAsync(string container, string path)
    {
        _logger.LogDebug("Loading Optional Text Resource: {Container}, {Path}", container, path);

        BlobContainerClient containerClient = _blobClient.GetBlobContainerClient(container);
        BlobClient blobClient = containerClient.GetBlobClient(path);

        bool exists = await blobClient.ExistsAsync();
        if (!exists)
        {
            return null;
        }
        
        return await ReadBlobDataAsync(blobClient);
    }

    private static async Task<string> ReadBlobDataAsync(BlobClient blobClient)
    {
        // Read all the text of the blob to a string
        await using var stream = await blobClient.OpenReadAsync();
        using var reader = new StreamReader(stream);
        string data = await reader.ReadToEndAsync();
        
        // This could be handy in the future for additional diagnostics: _context.AddBlock(new TextResourceBlock(path, data));
        
        return data;
    }

    public async Task UploadAsync(string container, string path, string content)
    {
        _logger.LogInformation("Uploading Blob: {Container}, {Path}", container, path);
        
        BlobClient blobClient = _blobClient.GetBlobContainerClient(container).GetBlobClient(path);
        
        await using MemoryStream stream = new(Encoding.UTF8.GetBytes(content));
        await blobClient.UploadAsync(stream, overwrite: true);
    }
}