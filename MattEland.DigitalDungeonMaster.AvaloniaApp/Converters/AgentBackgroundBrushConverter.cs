using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Converters;

public class AgentBackgroundBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string resourceName = "SemiOrange3";
        if (value is ChatMessage message)
        {
            value = message.Author;
        }
        
        if (value is string author)
        {
            resourceName = author switch
            {
                "World Builder" => "SemiGreen3",
                "Game Master" => "SemiViolet3",
                "Story Teller" => "SemiPurple3",
                _ => resourceName
            };
        }

        return App.Current.FindResource(resourceName);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}