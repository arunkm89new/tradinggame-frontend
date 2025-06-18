using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TradingGame.Models;

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

        // Convenience properties for UI
        public bool IsTypeBuy => TradeType == "BUY";
        public bool IsTypeSell => TradeType == "SELL";
        
        public Color OpenTradeButtonColor => IsTypeBuy ? 
            (Color)Application.Current.Resources["TradingBuyButton"] : 
            (Color)Application.Current.Resources["TradingSellButton"];

        public string OpenTradeButtonText => $"OPEN TRADE ? {CurrentPrice}";

        // Commands
        public ICommand ClosePopupCommand { get; }
        public ICommand SelectTradeSizeCommand { get; }
        public ICommand SelectLeverageCommand { get; }
        public ICommand OpenTradeCommand { get; }

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

        private void ExecuteOpenTrade()
        {
            // Extract the numeric value from the trade size (remove $ sign)
            string tradeSizeValue = SelectedTradeSize.Replace("$", "");
            
            // Log the trade details - in a real app, you would send this to a backend
            Console.WriteLine($"Opening {TradeType} trade:");
            Console.WriteLine($"Stock: {Stock?.Symbol} ({Stock?.Name})");
            Console.WriteLine($"Trade Size: {SelectedTradeSize}");
            Console.WriteLine($"Leverage: {SelectedLeverage}");
            Console.WriteLine($"Current Price: {CurrentPrice}");
            
            // Here you would typically call a service to create the trade
            // For example:
            // await _tradingService.CreateTradeAsync(new TradeRequest
            // {
            //     Symbol = Stock.Symbol,
            //     TradeType = TradeType,
            //     TradeSize = decimal.Parse(tradeSizeValue),
            //     Leverage = SelectedLeverage,
            //     EntryPrice = CurrentPrice
            // });
            
            // For now we'll just close the popup
            IsVisible = false;
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