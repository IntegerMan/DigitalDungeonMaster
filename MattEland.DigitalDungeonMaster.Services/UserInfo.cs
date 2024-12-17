namespace MattEland.DigitalDungeonMaster.Services;

public record UserInfo
{
    public required string Username { get; init; }
    public required byte[] PasswordHash { get; init; }
    public required byte[] PasswordSalt { get; init; }
    public bool IsAdmin { get; init; }
}