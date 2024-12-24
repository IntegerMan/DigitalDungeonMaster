using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using MattEland.DigitalDungeonMaster.ClientShared;
using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Selectors;

public class ChatMessageTemplateSelector : IDataTemplate
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification="Used by Avalonia")] 
    public bool SupportsRecycling => false;

    [Content]
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global", Justification="Set in XAML")]
    public Dictionary<string, IDataTemplate> Templates { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Control? Build(object? data)
    {
        ChatMessage message = (ChatMessage)data!;

        // This is pretty grody. It'd be better to have a way of requesting it, or taking it in as a constructor parameter, but that's hard to do with XAML.
        ApiClient client = App.GetService<ApiClient>();
        if (message.Author == client.Username)
        {
            return Templates["You"].Build(data);
        }
        return Templates["Agent"].Build(data);
    }

    public bool Match(object? data) => data is ChatMessage;
}