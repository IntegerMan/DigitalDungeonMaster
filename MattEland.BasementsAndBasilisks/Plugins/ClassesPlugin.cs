using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

public class ClassesPlugin
{
    [KernelFunction("GetClasses")]
    [Description("Gets a list of available classes in the game.")]
    [return: Description("A list of classes characters can play as")]
    public async Task<List<ClassModel>> GetClasses()
    {
        Console.WriteLine("GetClasses Called");
        return new List<ClassModel>() {
            new ClassModel() { Name = "Human" },
            new ClassModel() { Name = "Elf" },
            new ClassModel() { Name = "Dwarf" },
            new ClassModel() { Name = "Canadian" },
        };
    }

    public class ClassModel {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}