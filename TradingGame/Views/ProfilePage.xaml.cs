using Microsoft.Maui.Controls;
using TradingGame.ViewModels;

namespace TradingGame
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
            SetBannerId();
        }
        private void SetBannerId()
        {

           // AdView.AdsId = "ca-app-pub-3940256099942544/6300978111"; //test
           AdView.AdsId = "ca-app-pub-2536984180867150/9940063999"; //live

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Refresh the data when the page appears
            if (BindingContext is ProfileViewModel viewModel)
            {
                await viewModel.LoadDataAsync();
            }
        }
    }
}