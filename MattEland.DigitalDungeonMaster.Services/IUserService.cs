namespace MattEland.DigitalDungeonMaster.Services;

public interface IUserService
{
    Task RegisterAsync(string username, string password);
    Task<bool> LoginAsync(string username, string password);
}