namespace SpiritIsland.Maui;

public partial class NewGamePage : ContentPage {

	readonly NewGameModel _model;

	public NewGamePage() {
		InitializeComponent();

		_model = new NewGameModel(_builder);

		BindingContext = _model;
	}

	protected override bool OnBackButtonPressed() {
		if( _model.HasFocusAdversary ) {
			_model.EditAdversaryCancel.Execute( null );
			return true;
		}
		return base.OnBackButtonPressed();
	}

	#region Spirit Event Handlers

	// Show
	void OpenSpiritDialog_Tapped(object sender, TappedEventArgs e) {
		SpiritsPanel.IsVisible = true;
	}

	// Accept
	void AcceptSpiritButton_Tapped(object sender, TappedEventArgs e) {
		if( sender is not View view ) return;
		_model.SelectedSpirit = view.ClassId;
		SpiritsPanel.IsVisible = false;
		StartButton.IsEnabled = !string.IsNullOrEmpty(_model.SelectedSpirit);
	}

	// Cancel
	void Cancel_SpiritSelect(object sender, EventArgs e) {
		SpiritsPanel.IsVisible = false;
	}

	#endregion Spirit Event Handlers

	async void StartButton_Clicked( object sender, EventArgs e ) {

		if( !MainThread.IsMainThread )
			throw new Exception("not on main thread!");

		StartButton.IsEnabled = false;
		Activity.IsRunning = true;

		// Spirit
		_model.SaveSelectedSpiritAsRecent();

		// Get Board (or randomize)
		string board = (string)Board.SelectedItem;
		if (string.IsNullOrEmpty(board))
			board = _availalbeBoards[(int)(DateTime.Now.Ticks % _availalbeBoards.Length)];

		// Init Configuration
		var gc = new GameConfiguration()
			.ConfigSpirits(_model.SelectedSpirit!)
			.ConfigBoards(board)
			.ConfigCommandBeasts(CommandBeast.IsChecked)
			.ConfigAdversary(_model.SelectedAdversary.ToConfig() );
		gc.ShuffleNumber = (int)DateTime.Now.Ticks;

		var gameState = _builder.BuildGame(gc);

		// Set up the New Game Page
		SoloGamePage.Current?.Dispose();
		SoloGamePage.Current = new SoloGamePage( gameState );
		NavigationPage.SetHasNavigationBar(SoloGamePage.Current, false);
		MainPage.Current.ShowCurrentGameButton(true);
		Navigation.InsertPageBefore(SoloGamePage.Current, this);
		await Navigation.PopAsync();

		Activity.IsRunning = false;
		StartButton.IsEnabled = true;

	}

	#region private fields
	
	readonly string[] _availalbeBoards = ["A", "B", "C", "D", "E", "F"];
	readonly GameBuilder _builder = new GameBuilder(
		new Basegame.GameComponentProvider(),
		new BranchAndClaw.GameComponentProvider(),
		new FeatherAndFlame.GameComponentProvider(),
		new JaggedEarth.GameComponentProvider(),
		new NatureIncarnate.GameComponentProvider()
	);

	#endregion private fields

}
