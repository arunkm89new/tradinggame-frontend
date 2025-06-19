using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Timers;
using TradingGame.ViewModels;
using TradingGame.Models;

namespace TradingGame
{
    public partial class TradePage : ContentPage
    {
        private System.Timers.Timer _ltpTimer;
        private string _currentSymbol;
        private static readonly HttpClient _httpClient = new HttpClient();
        private string _currentPrice = "$0.00";

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
            UpdatePriceHeader(stock);
        }

        private void UpdatePriceHeader(StockModel stock)
        {
            // Set the currency/stock name header
            string assetName = stock.Name;
            string currencySymbol = stock.Symbol.Split(':').Length > 1 ? stock.Symbol.Split(':')[1] : stock.Symbol;
            
            if (currencySymbol.StartsWith("BTC"))
            {
                CurrencyLabel.Text = $"1 BTC = $105,473.17";
            }
            else if (currencySymbol.StartsWith("ETH"))
            {
                CurrencyLabel.Text = $"1 ETH = $3,125.92";
            }
            else
            {
                CurrencyLabel.Text = $"{assetName}";
            }
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
                    
                    // Also try to get 24h change
                    url = $"https://api.binance.com/api/v3/ticker/24hr?symbol={apiSymbol}";
                    response = await _httpClient.GetAsync(url);
                    
                    string priceChangePercent = "0.00";
                    string priceChange = "0.00";
                    
                    if (response.IsSuccessStatusCode)
                    {
                        json = await response.Content.ReadAsStringAsync();
                        var jsonDoc = System.Text.Json.JsonDocument.Parse(json);
                        priceChangePercent = jsonDoc.RootElement.GetProperty("priceChangePercent").GetString();
                        priceChange = jsonDoc.RootElement.GetProperty("priceChange").GetString();
                    }
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Format to match the mockup style
                        double priceChangeValue = double.Parse(priceChange);
                        double percentValue = double.Parse(priceChangePercent);
                        double currentPrice = double.Parse(price);
                        
                        // Format the price with commas for thousands
                        string formattedPrice = $"{currentPrice:N2}";
                        _currentPrice = $"$ {formattedPrice}";
                        
                        // Update the TradePopup price if it's open
                        if (BindingContext is TradePageViewModel vm)
                        {
                            vm.TradePopupVM.CurrentPrice = _currentPrice;
                        }
                        
                        // Update the CurrencyLabel with the real-time price
                        if (apiSymbol.StartsWith("BTC"))
                        {
                            CurrencyLabel.Text = $"1 BTC = ${formattedPrice}";
                        }
                        else if (apiSymbol.StartsWith("ETH"))
                        {
                            CurrencyLabel.Text = $"1 ETH = ${formattedPrice}";
                        }
                        else
                        {
                            CurrencyLabel.Text = $"{apiSymbol} = ${formattedPrice}";
                        }
                        
                        string priceChangeText = $"Today {(priceChangeValue >= 0 ? "+" : "")}{priceChange} ({(percentValue >= 0 ? "+" : "")}{priceChangePercent}%)";
                        PriceLabel.Text = priceChangeText;
                        
                        // Set color based on price change
                        PriceLabel.TextColor = percentValue >= 0 ? 
                            (Color)Application.Current.Resources["TradingPriceGreen"] : 
                            (Color)Application.Current.Resources["TradingPriceRed"];
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        PriceLabel.Text = "Today's change not available";
                    });
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PriceLabel.Text = "Price data unavailable";
                });
            }
        }

        private void OnBuyClicked(object sender, EventArgs e)
        {
            // Show trade popup for Buy
            if (BindingContext is TradePageViewModel vm)
            {
                vm.TradePopupVM.TradeType = "BUY";
                vm.TradePopupVM.Stock = vm.SelectedStock;
                vm.TradePopupVM.CurrentPrice = _currentPrice;
                vm.TradePopupVM.IsVisible = true;
                
                // Default selections
                vm.TradePopupVM.SelectedTradeSize = "$1500"; // As shown in mockup
                vm.TradePopupVM.SelectedLeverage = "30x";    // As shown in mockup
            }
        }

        private void OnSellClicked(object sender, EventArgs e)
        {
            // Show trade popup for Sell
            if (BindingContext is TradePageViewModel vm)
            {
                vm.TradePopupVM.TradeType = "SELL";
                vm.TradePopupVM.Stock = vm.SelectedStock;
                vm.TradePopupVM.CurrentPrice = _currentPrice;
                vm.TradePopupVM.IsVisible = true;
                
                // Default selections
                vm.TradePopupVM.SelectedTradeSize = "$1500"; // As shown in mockup
                vm.TradePopupVM.SelectedLeverage = "30x";    // As shown in mockup
            }
        }

        private async void OnCloseTradeClicked(object sender, EventArgs e)
        {
            // Close the previous order for the selected symbol, using the opposite trade type
            if (BindingContext is TradePageViewModel vm)
            {
                var tradeVM = vm.TradePopupVM;
                if (tradeVM.IsTradeOpen && tradeVM.Stock != null)
                {
                    tradeVM.CurrentPrice = _currentPrice;
                    if (tradeVM.CloseTradeCommand.CanExecute(null))
                    {
                        tradeVM.CloseTradeCommand.Execute(null);
                    }
                }
            }
        }
    }
}
