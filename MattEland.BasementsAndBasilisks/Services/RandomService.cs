using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattEland.BasementsAndBasilisks.Services;

public class RandomService
{
    private readonly Random _rand = new();

    public int GetRandomNumber(int max)
    {
        return _rand.Next(minValue: 1, maxValue: max + 1);
    }

    public int RollD6() => GetRandomNumber(6);
    public int RollD20() => GetRandomNumber(20);
    public int RollD20WithAdvantage() => Math.Max(GetRandomNumber(20), GetRandomNumber(20));
    public int RollD20WithDisadvantage() => Math.Min(GetRandomNumber(20), GetRandomNumber(20));
}