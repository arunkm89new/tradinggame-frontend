namespace TradingGame.Models
{
    public class StockModel
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public string IconPath => $"icon_{Name.ToLower()}.png";
        public string CurrentPrice { get; set; } = "$0.00";
        public string PriceChange { get; set; } = "+0.00";
        public string PriceChangePercent { get; set; } = "(0.00%)";
        public bool IsPriceUp { get; set; } = true;
        public string TimerValue { get; set; } = "33:32:13";
    }
}
