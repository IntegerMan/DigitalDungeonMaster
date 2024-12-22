using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using MattEland.DigitalDungeonMaster.AvaloniaApp.ViewModels;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        string name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        Type? type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = $"Not Found: {name}" };
    }

    public bool Match(object? data) 
        => data is ViewModelBase;
}