using System;
using Microsoft.Maui.Controls;

namespace TradingGame
{
    public partial class LearnTradingPage : ContentPage
    {
        public LearnTradingPage()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
