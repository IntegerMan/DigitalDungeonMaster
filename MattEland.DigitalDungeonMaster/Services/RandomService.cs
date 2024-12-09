namespace MattEland.DigitalDungeonMaster.Services;

public class RandomService
{
    private readonly ILogger<RandomService> _logger;
    private readonly Random _rand = new();

    public RandomService(ILogger<RandomService> logger)
    {
        _logger = logger;
    }

    public int GetRandomNumber(int max)
    {
        int number = _rand.Next(minValue: 1, maxValue: max + 1);
        
        _logger.LogInformation("Rolled a D{Max} and got {Number}", max, number);
        
        return number;
    }

    public int RollD6() => GetRandomNumber(6);
    public int RollD20() => GetRandomNumber(20);
    public int RollD20WithAdvantage() => Math.Max(GetRandomNumber(20), GetRandomNumber(20));
    public int RollD20WithDisadvantage() => Math.Min(GetRandomNumber(20), GetRandomNumber(20));
}