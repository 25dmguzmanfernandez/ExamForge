using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace ExamForge
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var decor = Window?.DecorView;
            if (decor is null) return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R &&
                decor.WindowInsetsController is { } controller)
            {
                var iconosOscuros = (int)WindowInsetsControllerAppearance.LightStatusBars;
                controller.SetSystemBarsAppearance(iconosOscuros, iconosOscuros);
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                decor.SystemUiFlags |= SystemUiFlags.LightStatusBar;
            }
        }
    }
}
