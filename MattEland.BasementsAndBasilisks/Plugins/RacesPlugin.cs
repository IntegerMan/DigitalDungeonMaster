using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

public class RacesPlugin
{
    [KernelFunction("GetRaces")]
    [Description("Gets a list of available races in the game.")]
    [return: Description("A list of races characters can play as")]
    public async Task<List<RaceModel>> GetRaces()
    {
        Console.WriteLine("GetRaces Called");
        return new List<RaceModel>() {
            new RaceModel() { Name = "Human" },
            new RaceModel() { Name = "Elf" },
            new RaceModel() { Name = "Dwarf" },
            new RaceModel() { Name = "Canadian" },
        };
    }

    public class RaceModel {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}