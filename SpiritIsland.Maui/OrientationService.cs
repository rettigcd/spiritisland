namespace SpiritIsland.Maui.Testing; 

public class OrientationService {

	public static DeviceOrientation GetOrientation() {
#if ANDROID
		//IWindowManager windowManager = Android.App.Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
		//SurfaceOrientation orientation = windowManager.DefaultDisplay.Rotation;
		//bool isLandscape = orientation == SurfaceOrientation.Rotation90 || orientation == SurfaceOrientation.Rotation270;
		//return isLandscape ? DeviceOrientation.Landscape : DeviceOrientation.Portrait;
		return DeviceOrientation.Undefined;
#elif IOS
		//UIInterfaceOrientation orientation = UIApplication.SharedApplication.StatusBarOrientation;
		//bool isPortrait = orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown;
		//return isPortrait ? DeviceOrientation.Portrait : DeviceOrientation.Landscape;
		return DeviceOrientation.Undefined;
#else
		return DeviceOrientation.Undefined;
#endif
	}

	public static void SetOrientation(DeviceOrientation orientation) {

#if ANDROID
		//Set ScreenOrientation to Landscape  
		//MainActivity.Instance.RequestedOrientation = orientation switch {
		//	DeviceOrientation.Landscape => ScreenOrientation.Landscape,
		//	DeviceOrientation.Portrait =>  ScreenOrientation.Portrait,
		//	_ => throw new InvalidOperationException("invalid orientation request"),
		//};
#elif IOS
//Set ScreenOrientation to Landscape  
		//UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)(UIInterfaceOrientation.LandscapeLeft)), new NSString("orientation"));
		////Set ScreenOrientation to Portrait  
		//UIDevice.CurrentDevice.SetValueForKey(NSNumber.FromNInt((int)(UIInterfaceOrientation.Portrait)), new NSString("orientation"));
#else
		throw new InvalidOperationException("Can only set orientation on Andrioid or IOS");
#endif

	}

}

public enum DeviceOrientation {
	Undefined,
	Landscape,
	Portrait
}