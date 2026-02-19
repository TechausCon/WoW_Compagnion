using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace WoWInsight.Mobile.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var param = parameter as string;
        if (value is bool b)
        {
            if (!string.IsNullOrEmpty(param))
            {
                var options = param.Split('|');
                if (options.Length == 2)
                {
                    var trueColor = options[0];
                    var falseColor = options[1];

                    if (trueColor == "Green") return Colors.Green;
                    if (trueColor == "Red") return Colors.Red;
                    // Fallback to simpler logic or resource lookup if needed.
                    // For now strict Green/Red
                }
            }
            return b ? Colors.Green : Colors.Red;
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
