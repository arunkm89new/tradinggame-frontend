using Microsoft.Maui.Controls;
using TradingGame.ViewModels;

namespace TradingGame
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
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