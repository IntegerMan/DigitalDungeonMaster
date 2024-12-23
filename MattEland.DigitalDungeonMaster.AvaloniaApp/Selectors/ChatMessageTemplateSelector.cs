using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Selectors;

public class ChatMessageTemplateSelector : IDataTemplate
{
    public bool SupportsRecycling => false;
    
    [Content]
    public Dictionary<string, IDataTemplate> Templates {get;} = new();

    public Control? Build(object data)
    {
        ChatMessage message = (ChatMessage)data;

        return message.Author == "You"
            ? Templates["You"].Build(data)
            : Templates["Agent"].Build(data);
    }

    public bool Match(object data) => data is ChatMessage;
}