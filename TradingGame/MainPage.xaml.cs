using TradingGame.Services;

namespace TradingGame
{
    public partial class MainPage : ContentPage
    {
        private readonly TradeService _tradeService;

        public MainPage()
        {
            InitializeComponent();
            _tradeService = new TradeService();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            TradeService.DeleteDatabase();
        }
    }
}
