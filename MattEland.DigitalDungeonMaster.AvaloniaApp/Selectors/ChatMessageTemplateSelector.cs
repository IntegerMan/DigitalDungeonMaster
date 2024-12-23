using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Selectors;

public class ChatMessageTemplateSelector : IDataTemplate
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification="Used by Avalonia")] 
    public bool SupportsRecycling => false;

    [Content]
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global", Justification="Set in XAML")]
    public Dictionary<string, IDataTemplate> Templates { get; } = new();

    public Control? Build(object? data)
    {
        ChatMessage message = (ChatMessage)data!;

        return message.Author == "You"
            ? Templates["You"].Build(data)
            : Templates["Agent"].Build(data);
    }

    public bool Match(object? data) => data is ChatMessage;
}