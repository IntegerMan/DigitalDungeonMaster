namespace MattEland.DigitalDungeonMaster.WebAPI.Models;

public record RegisterBody
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}