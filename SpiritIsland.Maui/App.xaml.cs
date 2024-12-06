
namespace SpiritIsland.Maui;

public partial class App : Application {

	public App() {
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState) {
		var mainPage = new MainPage();
		NavigationPage.SetHasNavigationBar(mainPage, false);
		MainPage = new NavigationPage(mainPage);
		return base.CreateWindow(activationState);
	}
}
