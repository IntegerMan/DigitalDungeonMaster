namespace MattEland.DigitalDungeonMaster.Services;

public record UserInfo
{
    public string Username { get; init; }
    public byte[] PasswordHash { get; init; }
    public byte[] PasswordSalt { get; init; }
    public bool IsAdmin { get; init; }
}