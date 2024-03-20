namespace SpiritIsland.Maui;

public partial class NewGamePage : ContentPage {

	public NewGamePage() {
		InitializeComponent();

		// Spirits.
		ss.ItemsSource = _builder.SpiritNames;
		RecentSpirits.ItemsSource = SavedRecentSpirits;

		Adversary.ItemsSource = (string[])["",.. _builder.AdversaryNames];
		Board.ItemsSource = _availalbeBoards;
	}

	void SaveRecentSpirit( string spirit) {
		string[] recentSpirits = [spirit, .. SavedRecentSpirits.Where(s => s != spirit).Take(3)];
		RecentSpirits.ItemsSource = recentSpirits;
		SavedRecentSpirits = recentSpirits;
	}
	string[] SavedRecentSpirits {
		get => [..Preferences.Default.Get(RecentSpiritsKey, "").Split(",").Where(s => !string.IsNullOrEmpty(s))];
		set => Preferences.Default.Set(RecentSpiritsKey,value.Join(","));
	}
	const string RecentSpiritsKey = "RecentSpirits";

	#region Control Event Handlers

	void Adversary_SelectedIndexChanged( object sender, EventArgs e ) {
		int index = Adversary.SelectedIndex;
		if(index != 0) {
			AdversaryLevel.IsEnabled = true;
			AdversaryLevel.IsVisible = true;
			string adversary = (string)Adversary.SelectedItem;
			AdversaryLevel.ItemsSource = _builder.BuildAdversary( new AdversaryConfig(adversary,0) ).Levels;
			AdversaryLevel.SelectedIndex = 0;
		} else {
			AdversaryLevel.IsEnabled = false;
			AdversaryLevel.IsVisible = false;
			AdversaryLevel.ItemsSource = Array.Empty<object>();
			_adversary = AdversaryConfig.NullAdversary;
		}

	}

	void AdversaryLevel_SelectedIndexChanged( object sender, EventArgs e ) {
		_adversary = Adversary.SelectedIndex == 0 ? AdversaryConfig.NullAdversary
			: new AdversaryConfig( (string)Adversary.SelectedItem, AdversaryLevel.SelectedIndex );
	}

	#endregion Control Event Handlers

	async void Button_Clicked( object sender, EventArgs e ) {

		StartButton.IsEnabled = false;
		Activity.IsRunning = true;
		await Task.Delay(500); // Let the UI show the Activity

		// Spirit
		string spirit = (string)Spirit.ClassId;
		SaveRecentSpirit( spirit );

		// Get Board (or randomize)
		string board = (string)Board.SelectedItem;
		if(string.IsNullOrEmpty( board ))
			board = _availalbeBoards[(int)(DateTime.Now.Ticks % _availalbeBoards.Length)];

		// Init Configuration
		var gc = new GameConfiguration()
			.ConfigSpirits( [spirit] )
			.ConfigBoards( [board] )
			.ConfigCommandBeasts( CommandBeast.IsChecked )
			.ConfigAdversary( _adversary );
		gc.ShuffleNumber = (int)DateTime.Now.Ticks;

		SinglePlayerGamePage.QueueNewGame( _builder.BuildGame( gc ) );

		await Shell.Current.GoToAsync("//GamePage");

		// This releases the thread and lets the page load.  Then comes back and turns it off.  (I think).
		Dispatcher.Dispatch(() => {
			Activity.IsRunning = false;
			StartButton.IsEnabled = true;
		});
	}

	#region private fields

	AdversaryConfig _adversary = AdversaryConfig.NullAdversary;

	readonly string[] _availalbeBoards = ["A", "B", "C", "D", "E", "F"];
	readonly GameBuilder _builder = new GameBuilder(
		new Basegame.GameComponentProvider(),
		new BranchAndClaw.GameComponentProvider(),
		new FeatherAndFlame.GameComponentProvider(),
		new JaggedEarth.GameComponentProvider(),
		new NatureIncarnate.GameComponentProvider()
	);

	#endregion private fields

	// Show
	void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e) {
		Spirits.IsVisible = true;
	}

	// Accept
	void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
		if(sender is not View view) return;
		SetCurrentSpirit(view.ClassId);
		Spirits.IsVisible = false;
		StartButton.IsEnabled = ! string.IsNullOrEmpty(Spirit.ClassId);
	}

	// Cancel
	void Cancel_SpiritSelect(object sender, EventArgs e) {
		Spirits.IsVisible = false;
	}

	void SetCurrentSpirit(string spiritName) {
		Spirit.Source = SpiritToButtonImgConverter.NameToImage(spiritName);
		Spirit.ClassId = spiritName;
	}

}