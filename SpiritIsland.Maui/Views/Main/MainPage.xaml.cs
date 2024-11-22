namespace SpiritIsland.Maui; 

public partial class MainPage : ContentPage {

	static public MainPage Current => _current ?? throw new ArgumentNullException(nameof(Current));
	static MainPage? _current;

	public MainPage() {
		_current = this;
		InitializeComponent();

		DeviceDisplay.Current.MainDisplayInfoChanged += Current_MainDisplayInfoChanged;
	}

	void Current_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e) {
		ReportOrientation();
	}

	void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e) {}

	void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {}

	void CircleDrag_Started(object sender, DragStartingEventArgs e) {
		e.Data.Properties.Add("IsValidShape", true);
		ReportOrientation();
	}

	static void ReportOrientation() {
		// Shell.Current.DisplayAlert("Orientation", DeviceDisplay.Current.MainDisplayInfo.Orientation.ToString(), "Ok");
		// IDeviceDislpay - add to use
	}

	void NewGameButton_Clicked(object sender, EventArgs e) {
		var page = new NewGamePage();
		NavigationPage.SetHasNavigationBar(page,false);
		_ = Navigation.PushAsync(page);
	}

	void CurrentGameButton_Clicked(object sender, EventArgs e) {
		var gamePage = SoloGamePage.Current;
		if(gamePage is null) return;
		Navigation.PushAsync(gamePage);
	}


	public void ShowCurrentGameButton(bool showIt) {
		CurrentGameButton.IsVisible = showIt;
	}

}
