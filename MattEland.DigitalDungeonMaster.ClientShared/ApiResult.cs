namespace MattEland.DigitalDungeonMaster.ClientShared;

public record ApiResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}