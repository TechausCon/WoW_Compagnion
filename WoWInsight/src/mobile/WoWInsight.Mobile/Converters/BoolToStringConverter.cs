using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace WoWInsight.Mobile.Converters;

public class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var param = parameter as string;
        var options = param?.Split('|');
        if (value is bool b)
        {
            if (options != null && options.Length == 2)
            {
                return b ? options[0] : options[1];
            }
            return b ? "True" : "False";
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
