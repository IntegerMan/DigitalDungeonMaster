using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public partial class InGameViewModel : ObservableObject
{
    [ObservableProperty]
    private string _username = "You"; // TODO: Get this from the API client
    
    // TODO: Hook me up to the current adventure for the title
    [ObservableProperty]
    private string _title = "Digital Dungeon Master";

    [ObservableProperty]
    private ObservableCollection<ChatMessage> _conversationHistory = new()
    {
        new ChatMessage()
        {
            Author = "Game Master",
            Message = "Welcome to the game. What would you like to do?"
        },
        new ChatMessage()
        {
            Author = "You",
            Message = "I cast fireball!"
        },
        new ChatMessage()
        {
            Author = "Game Master",
            Message = "You can't do that. You're a rogue."
        },
        new ChatMessage()
        {
            Author = "You",
            Message = "I cast sneak attack!"
        },
        new ChatMessage()
        {
            Author = "Game Master",
            Message = "That's more like it. Roll for damage."
        },
        new ChatMessage()
        {
            Author = "You",
            Message = "I got a natural 20!"
        },
        new ChatMessage()
        {
            Author = "Game Master",
            Message = "Critical hit! You've slain the dragon!"
        },
        new ChatMessage()
        {
            Author = "You",
            Message = "I'm the best!"
        }
    };
    
    // TODO: On load, should start conversation
    
    // TODO: Command for chatting
    
    // TODO: Handle chat results by adding to conversation history
}