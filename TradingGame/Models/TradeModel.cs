using System;
using SQLite;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace TradingGame.Models
{
    public class TradeModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string TradeType { get; set; } // BUY or SELL
        public decimal EntryPrice { get; set; }
        public decimal ExitPrice { get; set; }
        public decimal TradeSize { get; set; }
        public int Leverage { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }
        public bool IsOpen { get; set; }

        public string PnLPercent
        {
            get
            {
                if (EntryPrice == 0) return "0.00%";
                decimal percentChange = (ExitPrice - EntryPrice) / EntryPrice * 100;
                if (TradeType == "SELL")
                    percentChange = -percentChange;
                string sign = percentChange > 0 ? "+" : "";
                return $"{sign}{percentChange:0.00}%";
            }
        }

        public decimal PnLValue
        {
            get
            {
                if (EntryPrice == 0) return 0m;
                decimal pnl = (ExitPrice - EntryPrice) * TradeSize / EntryPrice * Leverage;
                if (TradeType == "SELL")
                    pnl = -pnl;
                return pnl;
            }
        }

        public Color PnLColor
        {
            get
            {
                if (EntryPrice == 0) return Colors.Black;
                bool isProfit = TradeType == "BUY" ? ExitPrice > EntryPrice : ExitPrice < EntryPrice;
                var key = isProfit ? "TradingPriceGreen" : "TradingPriceRed";
                if (Application.Current?.Resources?.TryGetValue(key, out var colorObj) == true && colorObj is Color color)
                    return color;
                return isProfit ? Colors.Green : Colors.Red;
            }
        }
    }
}
