namespace MattEland.DigitalDungeonMaster.ConsoleApp;

public record ApiResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}