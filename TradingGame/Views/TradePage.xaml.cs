using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using TradingGame.Models;
using TradingGame.Services;
using TradingGame.ViewModels;
using CommunityToolkit.Maui.Views;

namespace TradingGame
{
    public partial class TradePage : ContentPage
    {
        private System.Timers.Timer _ltpTimer;
        private string _currentSymbol;
        private static readonly HttpClient _httpClient = new HttpClient();
        private string _currentPrice = "$0.00";
        private readonly UserService _userService;
        private readonly TradeService _tradeService = new TradeService();

        public TradePage()
        {
            try
            {
                InitializeComponent();
                _userService = new UserService();
                // Do not call any async method here!
                SetBannerId();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TradePage.ctor] Exception: {ex}");
                Application.Current.MainPage.DisplayAlert("Error", $"TradePage failed to load: {ex.Message}", "OK");
            }
        }
       
        private void SetBannerId()
        {

            // AdView.AdsId = "ca-app-pub-3940256099942544/6300978111"; //test
            AdView.AdsId = "ca-app-pub-2536984180867150/9940063999"; //live

        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _userService.InitializeAsync();
                await UpdateCashBalanceDisplay();
                
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
                    await CheckAndSetOpenTradeForStock(vm.SelectedStock);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TradePage.OnAppearing] Exception: {ex}");
                Application.Current.MainPage.DisplayAlert("Error", $"TradePage failed to appear: {ex.Message}", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopLtpUpdates();
        }
        
        private async Task UpdateCashBalanceDisplay()
        {
            if (BindingContext is TradePageViewModel vm)
            {
                decimal balance = await _userService.GetCashBalanceAsync();
                vm.CashBalance = $"$ {balance:N2}";
            }
        }

        private async void OnStockSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is StockModel selectedStock)
            {
                LoadChartForStock(selectedStock);
                StartLtpUpdates(selectedStock.Symbol);
                await CheckAndSetOpenTradeForStock(selectedStock);
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
                        // Use the sign of the original string for color, not the parsed value
                        bool isNegative = priceChangePercent.Trim().StartsWith("-") || priceChange.Trim().StartsWith("-");
                        double priceChangeValue = double.Parse(priceChange.Trim().Replace("+", string.Empty), System.Globalization.CultureInfo.InvariantCulture);
                        double percentValue = double.Parse(priceChangePercent.Trim().Replace("+", string.Empty), System.Globalization.CultureInfo.InvariantCulture);
                        if (priceChangePercent.Trim().StartsWith("-")) percentValue = -Math.Abs(percentValue);
                        if (priceChange.Trim().StartsWith("-")) priceChangeValue = -Math.Abs(priceChangeValue);
                        double currentPrice = double.Parse(price, System.Globalization.CultureInfo.InvariantCulture);
                        
                        // Format the price with commas for thousands
                        string formattedPrice = $"{currentPrice:N2}";
                        _currentPrice = "$ " + formattedPrice;
                        
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
                        
                        // Set color based on sign, not value
                        PriceLabel.TextColor = isNegative ? 
                            (Color)Application.Current.Resources["TradingPriceRed"] : 
                            (Color)Application.Current.Resources["TradingPriceGreen"];
                        // ProfitLossLabel.TextColor is now handled by ViewModel binding
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
                // Always reset to default trade size and leverage when opening popup
                vm.TradePopupVM.SetDefaultTradeSizeAndLeverage();
                vm.TradePopupVM.TradeType = "BUY";
                vm.TradePopupVM.Stock = vm.SelectedStock;
                vm.TradePopupVM.CurrentPrice = _currentPrice;
                vm.TradePopupVM.IsVisible = true;
            }
        }

        private void OnSellClicked(object sender, EventArgs e)
        {
            // Show trade popup for Sell
            if (BindingContext is TradePageViewModel vm)
            {
                // Always reset to default trade size and leverage when opening popup
                vm.TradePopupVM.SetDefaultTradeSizeAndLeverage();
                vm.TradePopupVM.TradeType = "SELL";
                vm.TradePopupVM.Stock = vm.SelectedStock;
                vm.TradePopupVM.CurrentPrice = _currentPrice;
                vm.TradePopupVM.IsVisible = true;
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
                        // Get P/L before closing
                        decimal entryPrice = tradeVM.EntryPrice;
                        decimal exitPrice = decimal.Parse(_currentPrice.Replace("$", "").Replace(",", ""));
                        decimal tradeSize = tradeVM.OpenTrade?.TradeSize ?? 0;
                        int leverage = tradeVM.OpenTrade?.Leverage ?? 1;
                        string tradeType = tradeVM.OpenTrade?.TradeType ?? tradeVM.TradeType;
                        decimal pnl = (exitPrice - entryPrice) * tradeSize / entryPrice * leverage;
                        if (tradeType == "SELL") pnl = -pnl;

                        tradeVM.CloseTradeCommand.Execute(null);
                        // Update cash balance in DB
                        await _userService.UpdateCashBalanceAsync(pnl);
                        
                        // Update the displayed balance after closing the trade
                        await UpdateCashBalanceDisplay();
                        // Refresh open trade state for this stock
                        await CheckAndSetOpenTradeForStock(tradeVM.Stock);
                    }
                }
            }
        }
        
        private async void OnAddMoneyClicked(object sender, EventArgs e)
        {
            try
            {
                // Show the Add Funds popup
                var addFundsPopup = new VirtualTradingApp.Views.AddFundsPopup();

            // Subscribe to the AdCompleted event
            addFundsPopup.AdCompleted += async (sender, result) =>
            {
            if (result.Success)
            {
                // Add $1000 to the user's balance
                await _userService.UpdateCashBalanceAsync(1000);
            
            // Update the displayed balance
            await UpdateCashBalanceDisplay();
            
            // Show a confirmation message
            await DisplayAlert("Virtual Money Added", "Successfully added $1,000 to your account!", "OK");
                }
                else
                {
                    // Ad was not watched or failed
                    await DisplayAlert("Error", result.Message, "OK");
                }
            };

            // Show the popup using MAUI CommunityToolkit extension method
            this.ShowPopup(addFundsPopup);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding cash: {ex.Message}");
                await DisplayAlert("Error", $"Failed to add funds. Please try again later. Error message: {ex.Message}", "OK");
            }
        }
      

        private async Task CheckAndSetOpenTradeForStock(StockModel stock)
        {
            if (BindingContext is TradePageViewModel vm)
            {
                var openTrades = await _tradeService.GetOpenTradesAsync();
                var openTrade = openTrades.FirstOrDefault(t => t.Symbol == stock.Symbol);
                var tradeVM = vm.TradePopupVM;
                if (openTrade != null)
                {
                    // Set the open trade info for this stock
                    tradeVM.Stock = stock;
                    tradeVM.IsTradeOpen = true;
                    tradeVM.EntryPrice = openTrade.EntryPrice;
                    tradeVM.TradeType = openTrade.TradeType;
                    tradeVM.TradeOpenTime = openTrade.OpenTime;
                    tradeVM.SelectedTradeSize = "$" + openTrade.TradeSize.ToString("N0");
                    tradeVM.SelectedLeverage = openTrade.Leverage + "x";
                    tradeVM.OpenTrade = openTrade;
                    tradeVM.UpdateProfitLossPublic();
                }
                else
                {
                    // No open trade for this stock
                    tradeVM.IsTradeOpen = false;
                    tradeVM.OpenTrade = null;
                }
            }
        }
    }
}
