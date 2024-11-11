namespace SpiritIsland.Maui;

public partial class App : Application {

	public App() {

		InitializeComponent();

		// MainPage = new AppShell();
		var mainPage = new MainPage();
		NavigationPage.SetHasNavigationBar(mainPage,false);
		MainPage = new NavigationPage(mainPage);
	}

}
