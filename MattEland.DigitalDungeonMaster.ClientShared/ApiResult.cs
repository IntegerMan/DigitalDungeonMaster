namespace MattEland.DigitalDungeonMaster.ClientShared;

public record ApiResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResult Failure(string message) 
        => new()
        {
            Success = false,
            ErrorMessage = message
        };
}