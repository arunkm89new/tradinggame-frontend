using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Timers;

namespace TradingGame
{
    public partial class TradePage : ContentPage
    {
        private System.Timers.Timer _ltpTimer;
        private string _currentSymbol;
        private static readonly HttpClient _httpClient = new HttpClient();

        public TradePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Set default selection to first stock if not already selected
            if (BindingContext is TradePageViewModel vm && vm.Stocks.Count > 0)
            {
                if (vm.SelectedStock == null)
                {
                    vm.SelectedStock = vm.Stocks[0];
                }
                // Ensure UI selection matches ViewModel
                StockListView.SelectedItem = vm.SelectedStock;
                LoadChartForStock(vm.SelectedStock);
                StartLtpUpdates(vm.SelectedStock.Symbol);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopLtpUpdates();
        }

        private void OnStockSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is StockModel selectedStock)
            {
                LoadChartForStock(selectedStock);
                StartLtpUpdates(selectedStock.Symbol);
            }
            else
            {
                TradingViewWebView.Source = null;
                PriceLabel.Text = "Select a stock to see price";
                StopLtpUpdates();
            }
        }

        private void LoadChartForStock(StockModel stock)
        {
            if (stock == null) return;
            string symbol = stock.Symbol;
            string uniqueId = Guid.NewGuid().ToString("N"); // Unique for each load
            string html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0'>
    <style>
        html, body {{
            margin: 0;
            padding: 0;
            width: 100%;
            height: 100%;
            overflow: hidden;
        }}
        .tradingview-widget-container {{
            width: 100%;
            height: 100%;
            position: absolute;
        }}
        #tradingview_chart {{
            width: 100%;
            height: 100%;
        }}
    </style>
</head>
<body>
    <div class='tradingview-widget-container'>
        <div id='tradingview_chart'></div>
    </div>
    <script type='text/javascript' src='https://s3.tradingview.com/tv.js'></script>
    <script type='text/javascript'>
        document.addEventListener('DOMContentLoaded', function() {{
            new TradingView.widget({{
                'autosize': true,
                'symbol': '{symbol}',
                'interval': '1',
                'timezone': 'Etc/UTC',
                'theme': 'light',
                'style': '1',
                'locale': 'en',
                'toolbar_bg': '#f1f3f6',
                'enable_publishing': false,
                'withdateranges': true,
                'hide_side_toolbar': true,
                'allow_symbol_change': false,
                'container_id': 'tradingview_chart',
                'height': '100%',
                'width': '100%',
                'allowed_intervals': ['1','2','5']
            }});
        }});
    </script>
</body>
</html>";
            TradingViewWebView.Source = null; // Force reload
            TradingViewWebView.Source = new HtmlWebViewSource { Html = html };
            PriceLabel.Text = $"Selected: {stock.Name} ({stock.Symbol})";
        }

        private void StartLtpUpdates(string symbol)
        {
            _currentSymbol = symbol;
            StopLtpUpdates();
            _ltpTimer = new System.Timers.Timer(3000); // 3 seconds
            _ltpTimer.Elapsed += async (s, e) => await FetchAndUpdateLtp(_currentSymbol);
            _ltpTimer.AutoReset = true;
            _ltpTimer.Start();
            Task.Run(() => FetchAndUpdateLtp(symbol)); // Fetch immediately
        }

        private void StopLtpUpdates()
        {
            if (_ltpTimer != null)
            {
                _ltpTimer.Stop();
                _ltpTimer.Dispose();
                _ltpTimer = null;
            }
        }

        private async Task FetchAndUpdateLtp(string symbol)
        {
            try
            {
                // Example: Use a public API for crypto price (CoinGecko, Binance, etc.)
                // Here, we'll use Binance for BTCUSDT and ETHUSDT as a demo
                string apiSymbol = symbol.Replace("BINANCE:", "");
                string url = $"https://api.binance.com/api/v3/ticker/price?symbol={apiSymbol}";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var price = System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("price").GetString();
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        PriceLabel.Text = $"LTP: {price} USD";
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        PriceLabel.Text = "LTP: N/A";
                    });
                }
            }
            catch
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PriceLabel.Text = "LTP: N/A";
                });
            }
        }

        private void OnBuyClicked(object sender, EventArgs e)
        {
            // TODO: Show trade popup for Buy
        }

        private void OnSellClicked(object sender, EventArgs e)
        {
            // TODO: Show trade popup for Sell
        }

        private void OnCloseTradeClicked(object sender, EventArgs e)
        {
            // TODO: Close the trade
        }
    }
}
