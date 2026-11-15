
namespace SpiritIsland.Maui;

public partial class App : Application {

	public App() {
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState) {
		var mainPage = new MainPage();
		NavigationPage.SetHasNavigationBar(mainPage, false);
		var window = new Window(new NavigationPage(mainPage));

		// Keep the current game on disk whenever the app leaves the foreground - the OS may close it from there.
		window.Stopped += (_, _) => SuspendedGame.Save();
		window.Destroying += (_, _) => SuspendedGame.Save();
		return window;

	}
}
