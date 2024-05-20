//using Android.OS;
using SpiritIsland.Log;

namespace SpiritIsland.Maui;

public partial class SinglePlayerGamePage : ContentPage {

	#region static

	static SinglePlayerGamePage? _singleton;	// chicken
	static GameState? _unstartedGame;			// egg

	static public void QueueNewGame(GameState unstartedGameState) {
		_unstartedGame = unstartedGameState;	// set egg
		_singleton?.TryStartGame();				// tests for chicken
	}

	#endregion static

	public SinglePlayerGamePage() {
		InitializeComponent();
		_singleton = this;			// set chicken
		TryStartGame();				// tests for egg
	}

	#region Start/Stop Game

	void TryStartGame() {
		if(_unstartedGame is null) return;

		// Shut Down Old
		_model?.ShutDownOld();

		_model = new DecisionModel( _unstartedGame );
		_unstartedGame = null;

		_model.GameState.NewLogEntry += GameState_NewLogEntry;
		BindingContext = _model;

		// Start!
		_model.Start();
	}

	#endregion Start/Stop Game

	#region Send/Receive Commands to game

	async void GameState_NewLogEntry( Log.ILogEntry obj ) {
		if( obj is FearCardRevealed fcr)
			await DisplayAlert(fcr.Card.Text,fcr.GetInstructions(),"OK");

		else if( obj is IslandBlighted islandBlighted )
			await DisplayAlert(islandBlighted.Card.Text, islandBlighted.Card.Description, "OK" );

		else if( obj is CommandBeasts cb)
			await DisplayAlert( "Card Revealed", $"{cb.Title} - {cb.Desciption}", "OK"  );

		else if( obj is GameOverLogEntry go) {

			// !!! BIND this to a property so we can start new games
			GameOverInfo.Text = go.ToString();
			GameOverInfo.BackgroundColor = go.Result switch {
				GameOverResult.Victory => Colors.LightGreen,
				GameOverResult.Defeat => Colors.Pink,
				_ => Colors.Pink,
			};
			GameOverInfo.IsVisible = true;
			Prompt.IsVisible = Accept.IsVisible = OptionListWrapper.IsVisible = false;
		} else if( obj is Log.ExceptionEntry ) {

		}
	}

	#endregion Send/Receive Commands to game

	void SpiritSummary_GrowthDetailsClicked( object sender, EventArgs e ) => _model?.ShowSpiritPanel();
	void Accept_Clicked(object sender, EventArgs e) { 
		if(sender is not Button) return;
		_model?.Submit(); // !!! Replace with a command on the model
	}

	void SpiritSummary_CardDetailsClicked(object sender, EventArgs e) => _model?.ShowCardPanel();

	DecisionModel? _model;

}