using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TradingGame.Models;
using TradingGame.Services;

namespace TradingGame.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly TradeService _tradeService;
        private readonly UserService _userService;
        private string _userName = "Guest User";
        private string _location = "India";
        private string _portfolioValue = "$0";
        private string _returnPercentage = "0%";
        private string _totalTrades = "0";
        private bool _isLoading = true;
        private string _photoUrl = "profile_photo.png";

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public string Location
        {
            get => _location;
            set
            {
                if (_location != value)
                {
                    _location = value;
                    OnPropertyChanged(nameof(Location));
                }
            }
        }

        public string PortfolioValue
        {
            get => _portfolioValue;
            set
            {
                if (_portfolioValue != value)
                {
                    _portfolioValue = value;
                    OnPropertyChanged(nameof(PortfolioValue));
                }
            }
        }

        public string ReturnPercentage
        {
            get => _returnPercentage;
            set
            {
                if (_returnPercentage != value)
                {
                    _returnPercentage = value;
                    OnPropertyChanged(nameof(ReturnPercentage));
                    OnPropertyChanged(nameof(IsReturnNegative));
                }
            }
        }

        public string TotalTrades
        {
            get => _totalTrades;
            set
            {
                if (_totalTrades != value)
                {
                    _totalTrades = value;
                    OnPropertyChanged(nameof(TotalTrades));
                }
            }
        }

        public string PhotoUrl
        {
            get => _photoUrl;
            set
            {
                if (_photoUrl != value)
                {
                    _photoUrl = value;
                    OnPropertyChanged(nameof(PhotoUrl));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public bool IsReturnNegative => ReturnPercentage != null && ReturnPercentage.Contains("-");

        public ObservableCollection<TradeModel> ClosedTrades { get; } = new ObservableCollection<TradeModel>();

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }

        public ProfileViewModel()
        {
            _tradeService = new TradeService();
            _userService = new UserService();
            
            RefreshCommand = new Command(async () => await LoadDataAsync());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            
            Task.Run(async () => await LoadDataAsync());
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;

            try
            {
                var allTrades = await _tradeService.GetAllTradesAsync();
                var closed = allTrades.Where(t => !t.IsOpen)
                                      .OrderByDescending(t => t.CloseTime ?? DateTime.MinValue)
                                      .ToList();
                var cash = await _userService.GetCashBalanceAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ClosedTrades.Clear();
                    foreach (var trade in closed)
                    {
                        ClosedTrades.Add(trade);
                    }
                    // Update the counts
                    TotalTrades = ClosedTrades.Count.ToString();
                    PortfolioValue = "$" + cash.ToString("N0");
                    // Calculate total portfolio return percentage
                    if (ClosedTrades.Count > 0)
                    {
                        var totalProfitLoss = ClosedTrades.Sum(t => t.PnLValue);
                        var totalInvestment = ClosedTrades.Sum(t => t.TradeSize);
                        if (totalInvestment > 0)
                        {
                            var returnPercentage = (totalProfitLoss / totalInvestment) * 100;
                            ReturnPercentage = $"{(returnPercentage > 0 ? "+" : "")}{returnPercentage:0.00}%";
                        }
                        else
                        {
                            ReturnPercentage = "0.00%";
                        }
                    }
                    else
                    {
                        ReturnPercentage = "0.00%";
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading profile data: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}