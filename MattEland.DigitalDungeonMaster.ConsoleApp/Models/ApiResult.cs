namespace MattEland.DigitalDungeonMaster.ConsoleApp.Models;

public record ApiResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}