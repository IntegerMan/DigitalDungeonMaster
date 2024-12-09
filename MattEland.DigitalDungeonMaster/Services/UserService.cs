using System.Security.Cryptography;
using Azure.Data.Tables;

namespace MattEland.DigitalDungeonMaster.Services;

public class UserService
{
    private readonly StorageDataService _storageService;
    private readonly ILogger<UserService> _logger;
    private readonly string[] _restrictedUsernames = ["common", "admin", "administrator", "root", "shared"];

    public UserService(StorageDataService storageService, ILogger<UserService> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    private async Task<bool> UserExistsAsync(string? username)
    {
        return await _storageService.UserExistsAsync(username);
    }

    public async Task RegisterAsync(string username, string password)
    {
        _logger.LogInformation("Registering user {Username}", username);
        
        // We need to be able to reserve certain usernames for admin and shared features
        username = username.ToLowerInvariant();
        if (_restrictedUsernames.Contains(username))
        {
            _logger.LogWarning("User {Username} is restricted", username);
            throw new InvalidOperationException("This username is restricted. Please choose another.");
        }
        
        // Generate a random salt
        byte[] salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        
        byte[] hash = HashPassword(password, salt);

        // Store the salt and hash
        if (await UserExistsAsync(username))
        {
            _logger.LogWarning("User {Username} already exists", username);
            throw new InvalidOperationException("A user already exists with this username. Login instead.");
        }

        await _storageService.CreateTableEntryAsync("users", new TableEntity(username, username)
        {
            { "Salt", salt },
            { "Hash", hash }
        });
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        // Hash the password
        using var hasher = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        byte[] hash = hasher.GetBytes(32);
        return hash;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        _logger.LogInformation("Attempting to log in user {Username}", username);
        
        // Get the user
        (byte[]? salt, byte[]? hash) = await _storageService.GetUserSaltAndHash(username);
        if (salt == null || hash == null)
        {
            _logger.LogWarning("User {Username} not found", username);
            return false;
        }

        // Hash the password
        byte[] computedHash = HashPassword(password, salt);

        // Compare the hashes
        if (computedHash.SequenceEqual(hash))
        {
            _logger.LogInformation("User {Username} logged in successfully", username);
            return true;
        }
        else
        {
            _logger.LogWarning("User {Username} login failed with incorrect password", username);
            return false;
        }
    }
}