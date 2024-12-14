namespace MattEland.DigitalDungeonMaster.WebAPI.Models;

public record LoginBody
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}