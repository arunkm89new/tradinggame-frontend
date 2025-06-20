using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TradingGame.Models;

namespace TradingGame.ViewModels
{
    public class TradePageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<StockModel> Stocks { get; set; }
        private StockModel _selectedStock;
        public StockModel SelectedStock
        {
            get => _selectedStock;
            set
            {
                if (_selectedStock != value)
                {
                    _selectedStock = value;
                    OnPropertyChanged(nameof(SelectedStock));
                    
                    // Update the stock in the trade popup
                    if (TradePopupVM != null && _selectedStock != null)
                    {
                        TradePopupVM.Stock = _selectedStock;
                    }
                }
            }
        }

        public ICommand SelectStockCommand { get; private set; }
        public TradePopupViewModel TradePopupVM { get; private set; }

        public TradePageViewModel()
        {
            Stocks = new ObservableCollection<StockModel>
            {
                new StockModel { Name = "Bitcoin", Symbol = "BINANCE:BTCUSDT" },
                new StockModel { Name = "Ethereum", Symbol = "BINANCE:ETHUSDT" }
            };
            
            TradePopupVM = new TradePopupViewModel();
            
            // Command to handle stock selection from tap gesture
            SelectStockCommand = new Command<StockModel>(stock => 
            {
                if (stock != null)
                {
                    SelectedStock = stock;
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
