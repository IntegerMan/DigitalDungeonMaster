using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        string typeName = param.GetType().FullName!;
        
        // Try to find a view based on the view model's name
        string name = typeName.Replace("ViewModel", "View", StringComparison.Ordinal);
        Type? type = Type.GetType(name);

        // If the view model doesn't have a corresponding view, try a page
        if (type == null)
        {
            name = typeName.Replace("ViewModel", "Page", StringComparison.Ordinal);
            type = Type.GetType(name);
        }
        
        // If we found a view, create an instance of it
        if (type != null)
        {
            return (Control?)App.Current.Services.GetService(type) ?? throw new InvalidOperationException($"Could not create view: {type.FullName}");
        }

        throw new InvalidOperationException("Could not find view for view model: " + typeName);
    }

    public bool Match(object? data) 
        => data is ObservableObject;
}