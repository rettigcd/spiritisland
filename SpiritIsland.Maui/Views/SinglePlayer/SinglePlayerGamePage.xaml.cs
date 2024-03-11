//using Android.OS;
using SpiritIsland.SinglePlayer;
using SpiritIsland.Basegame;
using SpiritIsland.Log;

namespace SpiritIsland.Maui;

public partial class SinglePlayerGamePage : ContentPage {

	#region static

	static SinglePlayerGamePage? _singleton;
	static GameState? _unstartedGame;

	static public void QueueNewGame(GameState unstartedGameState) {
		_unstartedGame = unstartedGameState;
		_singleton?.TryStartGame();
	}

	#endregion static

	public SinglePlayerGamePage() {
		_singleton = this;

		InitializeComponent();

		BindingContext = _model;
		_ovm = new OptionViewManager( _model );
		IV.Ovm = _ovm;
		_model.Submitted += (sender,args) => OnAcceptClicked(sender!,args);
		

		TryStartGame();
	}

	#region Start/Stop Game

	void TryStartGame() {
		if(_unstartedGame is null) return;

		ShutDownOld();

		_gameState = _unstartedGame;
		_unstartedGame = null;

		//if(ActionScope.Current == null)
		//	ActionScope.Initialize( _gameState );

		// Setup the new game
		_game = new SinglePlayerGame( _gameState ) {
			LogExceptions = true,
			EnablePreselects = true,
		};
		Title = ((IOption)_game.Spirit).Text;

		// Wire up Game Notifications
		_userPortal = new UserPortalFacade( _game.UserPortal );
		_userPortal.NewWaitingDecision += Game_NewWaitingDecision;
		_gameState.NewLogEntry += GameState_NewLogEntry;
		
		var gs = _game.GameState;
		_model.GameStatus = new GameStatusModel(gs.InvaderDeck,gs.Fear, gs.Tokens[SpiritIsland.BlightCard.Space]);
		_model.GameStatus.Watch(_userPortal, gs);
		_model.SpiritSummary = new SpiritModel(_game.Spirit);

		// Cards
		_cardsModel = new CardsModel(_game.Spirit, _userPortal);
		_cardsModel.CardsSelected += _cardsModel_CardsSelected;
		Cards.BindingContext = _cardsModel;

		// Spirit Panel
		_spiritPanelModel?.Release();
		_spiritPanelModel = new SpiritPanelModel(_game.Spirit, _ovm, gs);
		SpiritPanel.BindingContext = _spiritPanelModel;

		// Controls
		IV.IslandTokens = _gameState.Tokens;
		IV.Board = _gameState.Island.Boards[0];

		// Start!
		_game.Start();
	}

	void _cardsModel_CardsSelected(IOption[] options) {
		_preSelectedOptions.UnionWith(options);
		AutoSelect();
	}

	void ShutDownOld() {
		if(_game is null) return;
		// release our event handlers
		if(_userPortal is not null) {
			_userPortal.NewWaitingDecision -= Game_NewWaitingDecision;
			_userPortal.Release();
		}

		IV.ReleaseTokens();

		ShowCardPanel(false);
		ShowSpiritPanel(false);

		if(_gameState is not null)
			_gameState.NewLogEntry -= GameState_NewLogEntry;

		// Quit the game and wait a bit
		_game.UserPortal.Quit();
		if(!_game.EngineTask.Wait( 500 )) {
			// !!! log that the task did not complete
		}
		_game = null;
		_gameState = null;

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

	void Game_NewWaitingDecision( IDecision decision ) {
		var gameState = _gameState!;

		// Update Prompt
		_nextDecision = decision;
		_model.Decision = _nextDecision;

		// Update Options
		OptionList.Children.Clear();
		foreach(var option in _nextDecision.Options)
			OptionList.Children.Add( new RadioButton { Value = option, Content = option.Text, GroupName = "DecisionOptions" } );

		// Board
		IV.SyncTokensToGameState();

		// Fear
		_model.FearPoolSize = gameState.Fear.PoolMax;
		_model.EarnedFear = gameState.Fear.EarnedFear;
		_model.FearCardsRemaining = gameState.Fear.CardsPerLevelRemaining;
		// Blight
		_model.BlightOnCard = gameState.Tokens[SpiritIsland.BlightCard.Space].Blight.Count;

		// Spirit
		_model.SpiritSummary?.Update(_ovm);

		_ovm.SyncOptionStatesToDecision(decision);

		if(AutoSelect()) return;

		if (_nextDecision is A.GrowthDecision) ShowSpiritPanel();
		if (_nextDecision is A.PowerCard) ShowCardPanel();
	}

	bool AutoSelect() {
		IOption? select = _nextDecision!.Options.FirstOrDefault(_preSelectedOptions.Contains);
		if (select is null) return false;
		_preSelectedOptions.Remove(select);
		_userPortal!.Choose(_nextDecision, select);
		return true;
	}

	void OnAcceptClicked( object sender, EventArgs e ) {
		if(_userPortal is null) { _model.AcceptText = "-- missing user portal --"; return; }
		if(_nextDecision is null) { _model.AcceptText = "-- missing decision --"; return; }
		if(_model.SelectedOption is null) { _model.AcceptText = "-no selection --"; return; }
		_userPortal.Choose( _nextDecision, (IOption)_model.SelectedOption );
	}

	#endregion Send/Receive Commands to game

	void SpiritSummary_GrowthDetailsClicked( object sender, EventArgs e ) => ShowSpiritPanel();
	void SpiritSummary_CardDetailsClicked(object sender, EventArgs e) => ShowCardPanel();

	void ShowSpiritPanel(bool show=true ) { SpiritPanel.IsVisible = show; }
	void ShowCardPanel(bool show=true ) { Cards.IsVisible = show; }

	readonly HashSet<IOption> _preSelectedOptions = [];

	#region private fields

	UserPortalFacade? _userPortal;
	IDecision? _nextDecision;
	GameState? _gameState;
	SinglePlayerGame? _game;

	SpiritPanelModel? _spiritPanelModel;
	CardsModel? _cardsModel;
	readonly DecisionModel _model = new DecisionModel( new GameState(new RiverSurges(),Board.BuildBoardA())); // throw away game

	readonly OptionViewManager _ovm;

	#endregion

}
