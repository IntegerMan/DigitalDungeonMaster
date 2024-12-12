namespace MattEland.DigitalDungeonMaster.Models;

public class HitPoints
{
    public HitPoints(int max) : this(max, max)
    {
    }

    public HitPoints(int current, int max, int temporary = 0)
    {
        Current = current;
        Max = max;
        Temporary = temporary;
    }

    public int Temporary { get; set; }

    public int Max { get; set; }

    public int Current { get; set; }
}