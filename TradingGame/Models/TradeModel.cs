using System;
using SQLite;

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
    }
}
