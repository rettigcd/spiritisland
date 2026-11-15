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
		// Shell.Current.DisplayAlertAsync("Orientation", DeviceDisplay.Current.MainDisplayInfo.Orientation.ToString(), "Ok");
		// IDeviceDislpay - add to use
	}

	void NewGameButton_Clicked(object sender, EventArgs e) {
		var page = new NewGamePage();
		NavigationPage.SetHasNavigationBar(page,false);
		_ = Navigation.PushAsync(page);
	}

	protected override void OnAppearing() {
		base.OnAppearing();
		RefreshResumeButton();
	}

	// Only offered when there is no game already open (the one open would be replaced).
	void RefreshResumeButton() {
		ResumeGameButton.IsVisible = SoloGamePage.Current is null && SuspendedGame.Exists;
	}

	void ResumeGameButton_Clicked(object sender, EventArgs e) {
		SoloGamePage page;
		try {
			page = SuspendedGame.Resume();
		} catch( Exception ex ) {
			// Unreadable or from an incompatible version - don't keep offering it.
			SuspendedGame.Delete();
			RefreshResumeButton();
			_ = DisplayAlertAsync("Resume", "The saved game could not be resumed: " + ex.Message, "OK");
			return;
		}

		SoloGamePage.Current = page;
		NavigationPage.SetHasNavigationBar(page, false);
		ShowCurrentGameButton(true);
		RefreshResumeButton();
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
