//using Android.OS;
using SpiritIsland.Log;

namespace SpiritIsland.Maui;

public partial class SinglePlayerGamePage : ContentPage, IDisposable {

	static public SinglePlayerGamePage? Current { get; set; }


	public SinglePlayerGamePage(GameState unstartedGame) {
		InitializeComponent();

		_model = new DecisionModel(unstartedGame);
		_model.GameState.NewLogEntry += GameState_NewLogEntry;
		BindingContext = _model;

		// Start!
		_model.Start();
	}


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
			ShowGameOver(go);
		} else if( obj is Log.ExceptionEntry ) {

		}
	}

	void ShowGameOver(GameOverLogEntry go) {
		GameOverInfo.Text = go.ToString();
		GameOverInfo.BackgroundColor = go.Result switch {
			GameOverResult.Victory => Colors.LightGreen,
			GameOverResult.Defeat => Colors.Pink,
			_ => Colors.Pink,
		};
		GameOverInfo.IsVisible = true;
		Prompt.IsVisible = Accept.IsVisible = OptionListWrapper.IsVisible = false;
	}

	#endregion Send/Receive Commands to game

	void SpiritSummary_GrowthDetailsClicked( object sender, EventArgs e ) => _model?.ShowSpiritPanel();
	void Accept_Clicked(object sender, EventArgs e) { 
		if(sender is not Button) return;
		_model?.Submit(); // !!! Replace with a command on the model
	}

	void SpiritSummary_CardDetailsClicked(object sender, EventArgs e) => _model?.ShowCardPanel();

	public void Dispose() {
		_model.ShutDownOld();
	}

	readonly DecisionModel _model;

	async void AdversaryButton_Clicked(object sender, EventArgs e) {
		await DisplayAlert("Adversary", _model.GameState.Adversary.Describe(),"Close");
	}
}