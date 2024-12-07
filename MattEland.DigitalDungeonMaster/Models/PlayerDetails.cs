namespace MattEland.DigitalDungeonMaster.Models;

public class PlayerDetails
{
    public string Name { get; set; }
    public int Level { get; set; }
    public string PlayerClass { get; set; }
    public required HitPoints HitPoints { get; init; }
    public string Description { get; set; }
    public string Goals { get; set; }
    public string Motivations { get; set; }
    public string Alignment { get; set; }
    public string Species { get; set; }
    public string Gender { get; set; }
    public string Nickname { get; set; }
}