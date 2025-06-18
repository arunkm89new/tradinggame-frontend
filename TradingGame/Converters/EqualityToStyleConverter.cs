using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TradingGame.Converters
{
    public class EqualityToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string paramString)
            {
                var parts = paramString.Split(';');
                if (parts.Length >= 3)
                {
                    string compareValue = parts[0];
                    string falseStyleName = parts[1];
                    string trueStyleName = parts[2];

                    bool isEqual = value?.ToString() == compareValue;
                    
                    if (Application.Current.Resources.TryGetValue(isEqual ? trueStyleName : falseStyleName, out var style))
                    {
                        return style;
                    }
                }
            }
            
            // Default style if nothing matches or something went wrong
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // MultiValue version for use with MultiBinding
    public class MultiValueEqualityToStyleConverter : BindableObject, IMultiValueConverter
    {
        public string StyleKey1 { get; set; } // Unselected
        public string StyleKey2 { get; set; } // Selected

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                var currentValue = values[0]?.ToString();
                var selectedValue = values[1]?.ToString();
                bool isEqual = currentValue == selectedValue;
                var styleKey = isEqual ? StyleKey2 : StyleKey1;
                if (!string.IsNullOrEmpty(styleKey) && Application.Current.Resources.TryGetValue(styleKey, out var style))
                {
                    return style;
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // MultiValue version for IsVisible
    public class MultiValueEqualityToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                var currentValue = values[0]?.ToString();
                var selectedValue = values[1]?.ToString();
                return currentValue == selectedValue;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}