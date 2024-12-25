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
    private readonly EventsService _events;

    public InGameViewModel(ApiClient client, ILogger<InGameViewModel> logger, NotificationService notify,
        EventsService events)
    {
        _client = client;
        _logger = logger;
        _notify = notify;
        _events = events;

        Username = _client.Username;
        Adventure = new AdventureInfo
        {
            Name = "Loading...",
            RowKey = "loading",
            Owner = Username,
            Status = AdventureStatus.Building,
            Description = "Please wait...",
        };

        ChatCommand = new AsyncRelayCommand(ChatAsync);

        Dispatcher.UIThread.Post(() => StartChatAsync(), DispatcherPriority.Default);
    }

    public Task StartChatAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;

        _logger.LogDebug("Loading adventure info");
        AdventureInfo adventure = _events.Request<AdventureInfo>();
        Adventure = adventure;
        _logger.LogDebug("Adventure info loaded {Name}", Adventure.RowKey);

        _logger.LogDebug("Starting chat with Game Master");
        return _client.StartGameMasterConversationAsync(adventure.RowKey, cancellationToken).ContinueWith(r =>
        {
            _logger.LogTrace("Received chat result. Moving to UI Thread");
            Dispatcher.UIThread.Invoke(() => { HandleChatResult(r, "Game Master"); });
        }, cancellationToken);
    }

    [ObservableProperty] private Guid _conversationId = Guid.Empty;
    [ObservableProperty] private AdventureInfo _adventure;

    [ObservableProperty] private string _username;

    [ObservableProperty] private ObservableCollection<ChatMessage> _conversationHistory = new();
    [ObservableProperty] private ObservableCollection<ChatMessage> _displayMessages = new();

    public AsyncRelayCommand ChatCommand { get; }

    private Task ChatAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Message))
        {
            _notify.ShowWarning("No Message", "Please enter a message to send");
            return Task.CompletedTask;
        }

        if (IsBusy)
        {
            _notify.ShowWarning("Busy", "Please wait for the current chat to complete");
            return Task.CompletedTask;
        }

        IsBusy = true;
        _logger.LogInformation("{User}: {Message}", Username, Message);

        ChatMessage yourMessage = new()
        {
            Author = Username,
            Message = Message
        };

        string recipient = SelectedAgent;

        GameChatRequest request = new()
        {
            Id = ConversationId,
            User = Username,
            Message = yourMessage,
            History = ConversationHistory.ToList(),
            RecipientName = recipient
        };

        // If the recipient is the game master, we want to add the message to the history. Otherwise, we want it to be a free request
        if (recipient == "Game Master")
        {
            ConversationHistory.Add(yourMessage);
        }

        DisplayMessages.Add(yourMessage);
        Message = string.Empty;

        return _client.ChatWithGameMasterAsync(request, Adventure.RowKey, cancellationToken)
            .ContinueWith(r =>
            {
                _logger.LogTrace("Received chat result. Moving to UI Thread");
                Dispatcher.UIThread.Invoke(() =>
                {
                    _logger.LogTrace("Now on UI Thread");
                    HandleChatResult(r, recipient);
                });
            }, cancellationToken);
    }

    private void HandleChatResult(Task<GameChatResult> r, string recipient)
    {
        _logger.LogDebug("Chat result: {Result}", r.Result);

        if (r.IsFaulted)
        {
            _logger.LogError(r.Exception, $"Failed to chat with {recipient}");
            _notify.ShowError("Failed to chat", $"An error occurred while chatting with the {recipient}");
        }
        else
        {
            ConversationId = r.Result.Id;

            if (r.Result.IsError)
            {
                _logger.LogError("Chat failed: {Message}", r.Result.ErrorMessage);
                _notify.ShowError("Chat Failed",
                    r.Result.ErrorMessage ?? $"An error occurred while chatting with the {recipient}");
            }
            else
            {
                _logger.LogDebug("Chat succeeded with {Count} replies", r.Result.Replies.Count());
                foreach (var reply in r.Result.Replies)
                {
                    _logger.LogInformation("{Agent}: {Message}", recipient, reply);
                    
                    // We only want to add responses from the game master to the history. Others are fine to display, but history goes back to the kernel
                    if (reply.Author == "Game Master")
                    {
                        ConversationHistory.Add(reply);
                    }

                    // Break the message into multiple messages by line breaks
                    string[] lines = reply.Message?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
                    foreach (var line in lines)
                    {
                        DisplayMessages.Add(new ChatMessage
                        {
                            Author = recipient,
                            Message = line
                            // TODO: Handle image responses
                        });
                    }
                }

                // The first time we get a result, it will include the available agents
                if (!AvailableAgents.Any())
                {
                    foreach (var agent in r.Result.AvailableAgents)
                    {
                        AvailableAgents.Add(agent);
                    }

                    _logger.LogDebug("Loaded {Count} available agents", AvailableAgents.Count);
                    SelectedAgent = AvailableAgents.First();
                    IsStarting = false;
                }
            }
        }

        IsBusy = false;
    }

    [ObservableProperty] private bool _isStarting = true;

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private string _message = string.Empty;

    public ObservableCollection<string> AvailableAgents { get; } = new();

    [ObservableProperty] private string _selectedAgent = "Loading...";
}