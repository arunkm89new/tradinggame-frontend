using CommunityToolkit.Maui.Views;
using Plugin.MauiMTAdmob;

namespace VirtualTradingApp.Views
{
    public class AdResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public partial class AddFundsPopup : Popup
    {
        public event EventHandler<AdResult> AdCompleted;

         // public string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // Test ad unit ID
        public string adUnitId = "ca-app-pub-2536984180867150/9293196737"; // live ad unit ID

        public AddFundsPopup()
        {
            InitializeComponent();
        }

        // In AddFundsPopup.xaml.cs, replace the OnWatchAdClicked method with:


        private async void OnWatchAdClicked(object sender, EventArgs e)
        {
            try
            {
                // Show the loader
                Loader.IsVisible = true;
                Loader.IsRunning = true;

                Console.WriteLine("Loading rewarded ad...");

                // Load the rewarded ad
                CrossMauiMTAdmob.Current.LoadRewarded(adUnitId);

                // Subscribe to events
                CrossMauiMTAdmob.Current.OnRewardedLoaded += (s, args) =>
                {
                    Console.WriteLine("Rewarded ad loaded successfully.");
                    CrossMauiMTAdmob.Current.ShowRewarded();
                    // Hide the loader
                    Loader.IsVisible = false;
                    Loader.IsRunning = false;
                };

                CrossMauiMTAdmob.Current.OnRewardedFailedToLoad += (s, args) =>
                {
                    AdCompleted?.Invoke(this, new AdResult
                    {
                        Success = false,
                        Message = args.ErrorMessage
                    });
                    // Hide the loader
                    Loader.IsVisible = false;
                    Loader.IsRunning = false;
                };

                CrossMauiMTAdmob.Current.OnUserEarnedReward += (s, args) =>
                {
                    AdCompleted?.Invoke(this, new AdResult
                    {
                        Success = true,
                        Message = $"Reward: {args.RewardAmount} {args.RewardType}"
                    });
                    // Hide the loader
                    Loader.IsVisible = false;
                    Loader.IsRunning = false;
                };

                CrossMauiMTAdmob.Current.OnRewardedClosed += (s, args) =>
                {
                    Console.WriteLine("Rewarded ad closed.");
                    Close(); // Close the popup when the ad is closed
                             // Hide the loader
                    Loader.IsVisible = false;
                    Loader.IsRunning = false;
                };


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing ad: {ex.Message}");
                AdCompleted?.Invoke(this, new AdResult
                {
                    Success = false,
                    Message = ex.Message
                });
                Close();
                // Hide the loader
                Loader.IsVisible = false;
                Loader.IsRunning = false;
            }
            
        }




        private void OnCancelClicked(object sender, EventArgs e)
        {
            // Close the popup without adding funds
            Close();
        }
    }
}
