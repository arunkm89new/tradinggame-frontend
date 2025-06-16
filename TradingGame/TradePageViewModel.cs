using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Intrinsics.X86;

namespace TradingGame
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
                }
            }
        }

        public TradePageViewModel()
        {
            Stocks = new ObservableCollection<StockModel>
            {
                new StockModel { Name = "Bitcoin", Symbol = "BINANCE:BTCUSDT" },
                new StockModel { Name = "Ethereum", Symbol = "BINANCE:ETHUSDT" }
                


            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
