using TradingGame.Services;

namespace TradingGame
{
    public partial class MainPage : ContentPage
    {
        private readonly TradeService _tradeService;

        public MainPage()
        {
            InitializeComponent();
            _tradeService = new TradeService();
        }

        private async void OnStartLearningClicked(object sender, EventArgs e)
        {
            // Navigate to the Learn Trading in 10 Minutes course page
            await Navigation.PushAsync(new LearnTradingPage());
        }

        private async void OnCourseSelected(object sender, EventArgs e)
        {
            // Course selection logic will be implemented when course navigation is set up
            if (sender is Button button)
            {
                string courseName = button.CommandParameter?.ToString() ?? "Selected course";
                await DisplayAlert("Course Selected", $"{courseName} will open here", "OK");
            }
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            TradeService.DeleteDatabase();
        }
    }
}
