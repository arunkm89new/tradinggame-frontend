using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TradingGame.Models;
using TradingGame.Services;
using System.Threading.Tasks;
using System.Timers;

namespace TradingGame.ViewModels
{
    public class TradePopupViewModel : INotifyPropertyChanged
    {
        private string _tradeType; // "BUY" or "SELL"
        private StockModel _stock;
        private string _currentPrice;
        private string _selectedTradeSize;
        private string _selectedLeverage;
        private bool _isVisible;
        private decimal _entryPrice;
        private DateTime? _tradeOpenTime;
        private bool _isTradeOpen;
        private string _profitLossText;
        private string _tradeDuration;
        private System.Timers.Timer _durationTimer;
        private TradeModel _openTrade;
        public TradeModel OpenTrade => _openTrade;
        private readonly TradeService _tradeService;
        private Color _profitLossColor = Colors.Black;
        private bool _isTradeSummaryVisible;
        private TradeSummaryViewModel _tradeSummary;

        // Trade size options
        public List<string> TradeSizeOptions { get; } = new List<string>
        {
            "$100", "$400", "$1000", "$1500", "$3000"
        };

        // Leverage options
        public List<string> LeverageOptions { get; } = new List<string>
        {
            "1x", "5x", "10x", "20x", "30x", "50x"
        };

        public string TradeType
        {
            get => _tradeType;
            set
            {
                if (_tradeType != value)
                {
                    _tradeType = value;
                    OnPropertyChanged(nameof(TradeType));
                    OnPropertyChanged(nameof(IsTypeBuy));
                    OnPropertyChanged(nameof(IsTypeSell));
                    OnPropertyChanged(nameof(OpenTradeButtonColor));
                }
            }
        }

        public StockModel Stock
        {
            get => _stock;
            set
            {
                if (_stock != value)
                {
                    _stock = value;
                    OnPropertyChanged(nameof(Stock));
                }
            }
        }

        public string CurrentPrice
        {
            get => _currentPrice;
            set
            {
                if (_currentPrice != value)
                {
                    _currentPrice = value;
                    OnPropertyChanged(nameof(CurrentPrice));
                    OnPropertyChanged(nameof(OpenTradeButtonText));
                    UpdateProfitLoss();
                }
            }
        }

        public string SelectedTradeSize
        {
            get => _selectedTradeSize;
            set
            {
                if (_selectedTradeSize != value)
                {
                    _selectedTradeSize = value;
                    OnPropertyChanged(nameof(SelectedTradeSize));
                }
            }
        }

        public string SelectedLeverage
        {
            get => _selectedLeverage;
            set
            {
                if (_selectedLeverage != value)
                {
                    _selectedLeverage = value;
                    OnPropertyChanged(nameof(SelectedLeverage));
                }
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        public decimal EntryPrice
        {
            get => _entryPrice;
            set { _entryPrice = value; OnPropertyChanged(nameof(EntryPrice)); }
        }
        public DateTime? TradeOpenTime
        {
            get => _tradeOpenTime;
            set { _tradeOpenTime = value; OnPropertyChanged(nameof(TradeOpenTime)); }
        }
        public bool IsTradeOpen
        {
            get => _isTradeOpen;
            set { _isTradeOpen = value; OnPropertyChanged(nameof(IsTradeOpen)); OnPropertyChanged(nameof(OpenTradeButtonText)); }
        }
        public string ProfitLossText
        {
            get => _profitLossText;
            set { _profitLossText = value; OnPropertyChanged(nameof(ProfitLossText)); }
        }
        public string TradeDuration
        {
            get => _tradeDuration;
            set { _tradeDuration = value; OnPropertyChanged(nameof(TradeDuration)); }
        }

        public Color ProfitLossColor
        {
            get => _profitLossColor;
            set { _profitLossColor = value; OnPropertyChanged(nameof(ProfitLossColor)); }
        }
        
        public bool IsTradeSummaryVisible
        {
            get => _isTradeSummaryVisible;
            set 
            { 
                _isTradeSummaryVisible = value; 
                OnPropertyChanged(nameof(IsTradeSummaryVisible)); 
            }
        }
        
        public TradeSummaryViewModel TradeSummary
        {
            get => _tradeSummary;
            set
            {
                _tradeSummary = value;
                OnPropertyChanged(nameof(TradeSummary));
            }
        }

        // Convenience properties for UI
        public bool IsTypeBuy => TradeType == "BUY";
        public bool IsTypeSell => TradeType == "SELL";
        
        public Color OpenTradeButtonColor => IsTypeBuy ? 
            (Color)Application.Current.Resources["TradingBuyButton"] : 
            (Color)Application.Current.Resources["TradingSellButton"];

        public string OpenTradeButtonText => IsTradeOpen ? "CLOSE TRADE" : $"OPEN TRADE ? {CurrentPrice}";

        // Commands
        public ICommand ClosePopupCommand { get; }
        public ICommand SelectTradeSizeCommand { get; }
        public ICommand SelectLeverageCommand { get; }
        public ICommand OpenTradeCommand { get; }
        public ICommand CloseTradeCommand { get; }
        public ICommand OkCommand { get; }
        public ICommand LearnCommand { get; }

        public ObservableCollection<ChipViewModel> TradeSizeChips { get; }
        public ObservableCollection<ChipViewModel> LeverageChips { get; }

        public TradePopupViewModel()
        {
            // Default values
            TradeType = "BUY";
            SelectedTradeSize = TradeSizeOptions[0];
            SelectedLeverage = LeverageOptions[0];
            CurrentPrice = "$ 0.00";
            IsVisible = false;
            IsTradeSummaryVisible = false;
            TradeSummary = new TradeSummaryViewModel();

            // Initialize chip collections
            TradeSizeChips = new ObservableCollection<ChipViewModel>();
            foreach (var size in TradeSizeOptions)
                TradeSizeChips.Add(new ChipViewModel { Value = size, IsSelected = size == SelectedTradeSize });
            LeverageChips = new ObservableCollection<ChipViewModel>();
            foreach (var lev in LeverageOptions)
                LeverageChips.Add(new ChipViewModel { Value = lev, IsSelected = lev == SelectedLeverage });

            // Initialize commands
            ClosePopupCommand = new Command(() => IsVisible = false);
            SelectTradeSizeCommand = new Command<ChipViewModel>(chip => SelectTradeSize(chip));
            SelectLeverageCommand = new Command<ChipViewModel>(chip => SelectLeverage(chip));
            OpenTradeCommand = new Command(ExecuteOpenTrade);
            CloseTradeCommand = new Command(async () => await ExecuteCloseTrade());
            OkCommand = new Command(() => IsTradeSummaryVisible = false);
            LearnCommand = new Command(ExecuteLearn);

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "trades.db3");
            _tradeService = new TradeService(dbPath);
        }

        private void ExecuteLearn()
        {
            // In a real app, this would navigate to a learning page
            // For now, just close the summary
            IsTradeSummaryVisible = false;
        }

        private void SelectTradeSize(ChipViewModel chip)
        {
            if (chip == null) return;
            foreach (var c in TradeSizeChips) c.IsSelected = false;
            chip.IsSelected = true;
            SelectedTradeSize = chip.Value;
        }

        private void SelectLeverage(ChipViewModel chip)
        {
            if (chip == null) return;
            foreach (var c in LeverageChips) c.IsSelected = false;
            chip.IsSelected = true;
            SelectedLeverage = chip.Value;
        }

        private async void ExecuteOpenTrade()
        {
            // Extract the numeric value from the trade size (remove $ sign)
            string tradeSizeValue = SelectedTradeSize.Replace("$", "");
            
            // Log the trade details - in a real app, you would send this to a backend
            Console.WriteLine($"Opening {TradeType} trade:");
            Console.WriteLine($"Stock: {Stock?.Symbol} ({Stock?.Name})");
            Console.WriteLine($"Trade Size: {SelectedTradeSize}");
            Console.WriteLine($"Leverage: {SelectedLeverage}");
            Console.WriteLine($"Current Price: {CurrentPrice}");
            
            // For now we'll just close the popup
            decimal entryPrice = decimal.Parse(CurrentPrice.Replace("$", "").Replace(",", ""));
            decimal tradeSize = decimal.Parse(SelectedTradeSize.Replace("$", "").Replace(",", ""));
            int leverage = int.Parse(SelectedLeverage.Replace("x", ""));
            var trade = new TradeModel
            {
                Symbol = Stock?.Symbol,
                Name = Stock?.Name,
                TradeType = TradeType,
                EntryPrice = entryPrice,
                TradeSize = tradeSize,
                Leverage = leverage,
                OpenTime = DateTime.UtcNow,
                IsOpen = true
            };
            await _tradeService.SaveTradeAsync(trade);
            _openTrade = trade;
            EntryPrice = entryPrice;
            TradeOpenTime = trade.OpenTime;
            IsTradeOpen = true;
            StartDurationTimer();
            UpdateProfitLoss();
            // Hide popup
            IsVisible = false;
        }

        private async Task ExecuteCloseTrade()
        {
            if (_openTrade == null) return;
            
            decimal exitPrice = decimal.Parse(CurrentPrice.Replace("$", "").Replace(",", ""));
            await _tradeService.CloseTradeAsync(_openTrade, exitPrice);
            
            // Calculate P/L for the summary
            decimal pnl = (exitPrice - EntryPrice) * _openTrade.TradeSize / EntryPrice * _openTrade.Leverage;
            if (_openTrade.TradeType == "SELL") pnl = -pnl;
            
            // Set up the trade summary data
            TradeSummary.AssetName = Stock?.Name;
            TradeSummary.TradeType = _openTrade.TradeType;
            TradeSummary.TradeSize = _openTrade.TradeSize;
            TradeSummary.Leverage = _openTrade.Leverage;
            TradeSummary.EntryPrice = _openTrade.EntryPrice;
            TradeSummary.ExitPrice = exitPrice;
            TradeSummary.ProfitLoss = pnl;
            
            // Calculate duration
            TimeSpan duration = DateTime.UtcNow - _openTrade.OpenTime;
            TradeSummary.Duration = FormatDuration(duration);
            
            // Calculate commission (example: 0.1% of trade size)
           // TradeSummary.Commission = _openTrade.TradeSize * 0.001m;
            
            // Show the trade summary popup
            IsTradeSummaryVisible = true;
            
            // Reset the trade state
            IsTradeOpen = false;
            StopDurationTimer();
        }

        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{(int)duration.TotalDays} days";
            }
            else if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours} hours";
            }
            else if (duration.TotalMinutes >= 1)
            {
                return $"{(int)duration.TotalMinutes} mins";
            }
            else
            {
                return $"{(int)duration.TotalSeconds} secs";
            }
        }

        private void StartDurationTimer()
        {
            _durationTimer = new System.Timers.Timer(1000);
            _durationTimer.Elapsed += (s, e) => UpdateDuration();
            _durationTimer.Start();
        }
        private void StopDurationTimer()
        {
            _durationTimer?.Stop();
            _durationTimer = null;
        }
        private void UpdateDuration()
        {
            if (TradeOpenTime == null) return;
            var duration = DateTime.UtcNow - TradeOpenTime.Value;
            TradeDuration = duration.ToString(@"hh\:mm\:ss");
        }
        private void UpdateProfitLoss()
        {
            if (!IsTradeOpen) { ProfitLossText = ""; ProfitLossColor = Colors.Black; return; }
            decimal currentPrice = decimal.Parse(CurrentPrice.Replace("$", "").Replace(",", ""));
            decimal pnl = (currentPrice - EntryPrice) * _openTrade.TradeSize / EntryPrice * _openTrade.Leverage;
            if (_openTrade.TradeType == "SELL") pnl = -pnl;
            decimal percent = (pnl / (_openTrade.TradeSize * _openTrade.Leverage)) * 100;
            ProfitLossText = $"P/L: {pnl:F2} ({percent:F2}%)";
            ProfitLossColor = pnl < 0 ? Colors.Red : Colors.Green;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class TradeSummaryViewModel : INotifyPropertyChanged
    {
        private string _assetName;
        private string _tradeType;
        private decimal _tradeSize;
        private int _leverage;
        private decimal _entryPrice;
        private decimal _exitPrice;
        private decimal _profitLoss;
        private string _duration;
        private decimal _commission;
        private string _headerText;
        private string _profitLossDisplay;
        private Color _profitLossColor;

        public string AssetName
        {
            get => _assetName;
            set
            {
                _assetName = value;
                OnPropertyChanged(nameof(AssetName));
            }
        }

        public string TradeType
        {
            get => _tradeType;
            set
            {
                _tradeType = value;
                OnPropertyChanged(nameof(TradeType));
            }
        }

        public decimal TradeSize
        {
            get => _tradeSize;
            set
            {
                _tradeSize = value;
                OnPropertyChanged(nameof(TradeSize));
                OnPropertyChanged(nameof(TradeSizeDisplay));
            }
        }

        public int Leverage
        {
            get => _leverage;
            set
            {
                _leverage = value;
                OnPropertyChanged(nameof(Leverage));
                OnPropertyChanged(nameof(LeverageDisplay));
            }
        }

        public decimal EntryPrice
        {
            get => _entryPrice;
            set
            {
                _entryPrice = value;
                OnPropertyChanged(nameof(EntryPrice));
                OnPropertyChanged(nameof(EntryPriceDisplay));
            }
        }

        public decimal ExitPrice
        {
            get => _exitPrice;
            set
            {
                _exitPrice = value;
                OnPropertyChanged(nameof(ExitPrice));
                OnPropertyChanged(nameof(ExitPriceDisplay));
            }
        }

        public decimal ProfitLoss
        {
            get => _profitLoss;
            set
            {
                _profitLoss = value;
                ProfitLossColor = value < 0 ? Colors.Red : Colors.Green;
                ProfitLossDisplay = $"{(value < 0 ? "-" : "")}${Math.Abs(value):F2}";
                HeaderText = value < 0 ? "Unfortunately, You've lost!" : "Congratulations, You've won!";
                OnPropertyChanged(nameof(ProfitLoss));
            }
        }

        public string Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        public decimal Commission
        {
            get => _commission;
            set
            {
                _commission = value;
                OnPropertyChanged(nameof(Commission));
                OnPropertyChanged(nameof(CommissionDisplay));
            }
        }

        // Formatted display properties
        public string TradeSizeDisplay => $"$ {TradeSize}";
        public string LeverageDisplay => $"x{Leverage}";
        public string EntryPriceDisplay => $"$ {EntryPrice:N2}";
        public string ExitPriceDisplay => $"$ {ExitPrice:N2}";
        public string CommissionDisplay => $"$ {Commission:F4}";

        public string HeaderText
        {
            get => _headerText;
            set
            {
                _headerText = value;
                OnPropertyChanged(nameof(HeaderText));
            }
        }

        public string ProfitLossDisplay
        {
            get => _profitLossDisplay;
            set
            {
                _profitLossDisplay = value;
                OnPropertyChanged(nameof(ProfitLossDisplay));
            }
        }

        public Color ProfitLossColor
        {
            get => _profitLossColor;
            set
            {
                _profitLossColor = value;
                OnPropertyChanged(nameof(ProfitLossColor));
            }
        }

        public TradeSummaryViewModel()
        {
            ProfitLossColor = Colors.Red; // Default
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ChipViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        public string Value { get; set; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}