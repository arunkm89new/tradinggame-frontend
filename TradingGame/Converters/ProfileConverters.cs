using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace TradingGame.Converters
{
    public class PnLConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 1 || parameter == null)
            {
                return 0;
            }

            try
            {
                // Extract the parameters
                var paramArray = parameter.ToString().Split(',');
                if (paramArray.Length < 4)
                {
                    return 0;
                }

                decimal exitPrice = (decimal)values[0];
                decimal entryPrice = decimal.Parse(paramArray[0]);
                decimal tradeSize = decimal.Parse(paramArray[1]);
                int leverage = int.Parse(paramArray[2]);
                string tradeType = paramArray[3];

                // Calculate P/L
                decimal pnl = (exitPrice - entryPrice) * tradeSize / entryPrice * leverage;
                if (tradeType == "SELL")
                {
                    pnl = -pnl;
                }

                return pnl;
            }
            catch
            {
                return 0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PnLPercentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 1 || parameter == null)
            {
                return 0;
            }

            try
            {
                // Extract the parameters
                var paramArray = parameter.ToString().Split(',');
                if (paramArray.Length < 2)
                {
                    return 0;
                }

                decimal exitPrice = (decimal)values[0];
                decimal entryPrice = decimal.Parse(paramArray[0]);
                string tradeType = paramArray[1];

                // Calculate percentage change
                decimal percentChange = (exitPrice - entryPrice) / entryPrice * 100;
                if (tradeType == "SELL")
                {
                    percentChange = -percentChange;
                }

                return percentChange;
            }
            catch
            {
                return 0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PnLColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                return Colors.Black;
            }

            try
            {
                decimal exitPrice = (decimal)values[0];
                decimal entryPrice = (decimal)values[1];
                string tradeType = (string)values[2];

                // Determine if profit or loss
                bool isProfit = tradeType == "BUY" ? exitPrice > entryPrice : exitPrice < entryPrice;

                // Return appropriate color
                if (isProfit)
                {
                    return Application.Current.Resources["TradingPriceGreen"];
                }
                else
                {
                    return Application.Current.Resources["TradingPriceRed"];
                }
            }
            catch
            {
                return Colors.Black;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TradeTypeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string tradeType)
            {
                return tradeType == "BUY" ?
                    Application.Current.Resources["TradingBuyButton"] :
                    Application.Current.Resources["TradingSellButton"];
            }
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorParams)
            {
                var colors = colorParams.Split(',');
                if (colors.Length < 2)
                    return Colors.Black;

                if (boolValue)
                {
                    return Application.Current.Resources[colors[0]];
                }
                else
                {
                    return Application.Current.Resources[colors[1]];
                }
            }
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}