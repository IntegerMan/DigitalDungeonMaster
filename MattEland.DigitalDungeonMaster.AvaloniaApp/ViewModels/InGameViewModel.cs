using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MattEland.DigitalDungeonMaster.AvaloniaApp.Services;
using MattEland.DigitalDungeonMaster.ClientShared;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.Logging;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

public partial class InGameViewModel : ObservableObject
{
    private readonly ApiClient _client;
    private readonly ILogger<InGameViewModel> _logger;
    private readonly NotificationService _notify;

    public InGameViewModel(ApiClient client, ILogger<InGameViewModel> logger, NotificationService notify)
    {
        _client = client;
        _logger = logger;
        _notify = notify;

        Username = _client.Username;
        Adventure = new AdventureInfo // TODO: Get this from another service
        {
            Name = "Test",
            RowKey = "unknownlands",
            Owner = "meland",
            Ruleset = "dnd5e",
            Status = AdventureStatus.InProgress,
            Description = "A test adventure",
        };

        ChatCommand = new AsyncRelayCommand(ChatAsync);

        // TODO: Start chat on load
    }

    [ObservableProperty]
    private AdventureInfo _adventure;

    [ObservableProperty]
    private string _username;

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
        }
    };

    public AsyncRelayCommand ChatCommand { get; }

    private Task ChatAsync(CancellationToken arg)
    {
        _logger.LogInformation("{User}: {Message}", Username, Message);
        
        ChatMessage yourMessage = new()
        {
            Author = "You",
            Message = Message
        };
        
        string recipient = "Game Master"; // TODO: Get from ComboBox
        
        ChatRequest<object> request = new()
        {
            Id = Guid.NewGuid(), // TODO: Get from initial chat
            User = Username,
            Message = yourMessage.Message,
            Data = null,
            History = ConversationHistory.ToList(),
            RecipientName = recipient
        };
        
        ConversationHistory.Add(yourMessage);
        Message = string.Empty;

        return _client.ChatWithGameMasterAsync(request, Adventure.RowKey).ContinueWith(r =>
        {
            _logger.LogTrace("Received chat result. Moving to UI Thread");
            Dispatcher.UIThread.Invoke(() =>
            {
                _logger.LogTrace("Now on UI Thread");
                _logger.LogDebug("Chat result: {Result}", r.Result);
                
                if (r.IsFaulted)
                {
                    _logger.LogError(r.Exception, $"Failed to chat with {recipient}");
                    _notify.ShowError("Failed to chat", $"An error occurred while chatting with the {recipient}");
                } 
                else if (r.Result.IsError)
                {
                    _logger.LogError("Chat failed: {Message}", r.Result.ErrorMessage);
                    _notify.ShowError("Chat Failed", r.Result.ErrorMessage ?? $"An error occurred while chatting with the {recipient}");
                }
                else
                {
                    _logger.LogDebug("Chat succeeded with {Count} replies", r.Result.Replies?.Count() ?? 0);
                    foreach (var reply in r.Result.Replies ?? [])
                    {
                        _logger.LogInformation("{Agent}: {Message}", recipient, reply);
                        ConversationHistory.Add(reply);
                    }
                }
                
                IsBusy = false;
            });
        });
    }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _message = string.Empty;
}