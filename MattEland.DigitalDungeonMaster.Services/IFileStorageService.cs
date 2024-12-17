namespace MattEland.DigitalDungeonMaster.Services;

public interface IFileStorageService
{
    Task UploadAsync(string container, string path, string content);
    Task<string?> LoadTextOrDefaultAsync(string container, string path);
    Task<string> LoadTextAsync(string container, string path);
}