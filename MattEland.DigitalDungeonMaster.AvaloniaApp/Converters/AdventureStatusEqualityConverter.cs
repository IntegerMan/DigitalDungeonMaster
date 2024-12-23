using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MattEland.DigitalDungeonMaster.Shared;

namespace MattEland.DigitalDungeonMaster.AvaloniaApp.Converters;

public class AdventureStatusEqualityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AdventureStatus status && parameter is AdventureStatus targetStatus)
        {
            return status == targetStatus;
        }       
        if (value is AdventureInfo info && parameter is AdventureStatus targetStatus2)
        {
            return info.Status == targetStatus2;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}