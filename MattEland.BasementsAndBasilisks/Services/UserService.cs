namespace MattEland.BasementsAndBasilisks.Services;

public class UserService
{
    private readonly StorageDataService _storageService;

    public UserService(StorageDataService storageService)
    {
        _storageService = storageService;
    }
    
    public async Task<bool> UserExistsAsync(string? username)
    {
        return await _storageService.UserExistsAsync(username);
    }
}