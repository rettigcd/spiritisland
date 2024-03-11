//using Android.OS;
namespace SpiritIsland.Maui;

public class TestWindowOverlay : WindowOverlay {
	readonly IWindowOverlayElement _testWindowDrawable;

	public TestWindowOverlay(Window window) : base(window) {
		
		_testWindowDrawable = new EllipseWindowOverlay();
		AddWindowElement( _testWindowDrawable );

		EnableDrawableTouchHandling = true;
		Tapped += OnTapped;
	}

	async void OnTapped(object? sender, WindowOverlayTappedEventArgs e) {
		if (!e.WindowOverlayElements.Contains(_testWindowDrawable))
			return;

		System.Diagnostics.Debug.WriteLine($"Tapped the test overlay button.");

		Window? window = Application.Current!.Windows.FirstOrDefault(w => w == Window);
		if(window is null) return;

		string result = await window.Page!.DisplayActionSheet(
			"Greetings from Visual Studio Client Experiences!",
			"Goodbye!",
			null,
			"Do something", "Do something else", "Do something... with feeling.");
		System.Diagnostics.Debug.WriteLine(result);
	}
}
