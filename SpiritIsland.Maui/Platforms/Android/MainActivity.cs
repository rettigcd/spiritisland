using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SpiritIsland.Maui;

[Activity( 
	Theme = "@style/Maui.SplashTheme",
//	ScreenOrientation = ScreenOrientation.Landscape,
	MainLauncher = true, 
	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density 
)]
public class MainActivity : MauiAppCompatActivity {

	protected override void OnCreate(Bundle? savedInstanceState) {
		base.OnCreate(savedInstanceState);
		// RequestedOrientation = ScreenOrientation.Landscape;
	}

}
