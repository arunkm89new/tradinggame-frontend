using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Plugin.MauiMTAdmob;

namespace TradingGame
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Initialize Google Mobile Ads SDK
            MobileAds.Initialize(this);

            // Initialize MauiMTAdmob with your AdMob App ID
            string appId = "ca-app-pub-2536984180867150~4639318624"; // Replace with your actual AdMob App ID
            CrossMauiMTAdmob.Current.Init(this, appId);
        }
    }
}
