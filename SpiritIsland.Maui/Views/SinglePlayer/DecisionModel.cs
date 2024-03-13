//using Android.OS;
using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Maui;

public class DecisionModel : ObservableModel {

	// * NOT USED - but required to Make child models compile
	public bool IsVisible {get; set;}

	#region Observable properties

	public string Title {  get => _title; set => SetProp(ref _title,value); }
	string _title = "";

	public SpiritModel? SpiritSummary { get => _spiritSummary; set => SetProp(ref _spiritSummary, value); }

	public SpiritPanelModel? SpiritPanel { get => _spiritPanelModel; set => SetProp(ref _spiritPanelModel, value); }

	public CardsModel? Cards { get => _cardsModel; set => SetProp(ref _cardsModel, value); }

	public GameStatusModel GameStatus { get => _gameStatus; set => SetProp(ref _gameStatus, value); }

	public string Spaces { get => _spaces; set => SetProp(ref _spaces, value ); }

	public string Prompt {
		get => _prompt;
		set => SetProp(ref _prompt, value);
	}

	public IOption? SelectedOption {
		get => _option;
		set {
			SetProp( ref _option, value );
			if(value is not null) {
				HasOptionReady = true;
				AcceptText = ((IOption)value).Text;
			} else {
				HasOptionReady = false;
				AcceptText = "Accept";
			}
		}
	}

	public bool HasOptionReady { get => _hasOptionReady; set => SetProp( ref _hasOptionReady, value ); }
	public string AcceptText { get => _acceptText; set => SetProp( ref _acceptText, value); }

	GameStatusModel _gameStatus;
	SpiritPanelModel? _spiritPanelModel;
	CardsModel? _cardsModel;
	SpiritModel? _spiritSummary;
	string _prompt = string.Empty;
	IOption? _option;
	bool _hasOptionReady;
	string _acceptText;
	string _spaces = "";

	#endregion

	public IDecision Decision { 
		set {
			_nextDecision = value;
			Prompt = _nextDecision.Prompt;
			SelectedOption = null;
		}
	}

	public OptionModel[] Options { get => _options; set => SetProp( ref _options, value ); }
	OptionModel[] _options = [];

	/// <summary> Submits the currently selected Option as the Decisions choice. </summary>
	public void Submit() {
		if (_userPortal is null) { AcceptText = "-- missing user portal --"; return; }
		if (_nextDecision is null) { AcceptText = "-- missing decision --"; return; }
		if (SelectedOption is null) { AcceptText = "-no selection --"; return; }
		_userPortal.Choose(_nextDecision, (IOption)SelectedOption);
	}

	public bool AutoSelect() {
		IOption? select = _nextDecision!.Options.FirstOrDefault(_preSelectedOptions.Contains);
		if (select is null) return false;
		_preSelectedOptions.Remove(select);
		_userPortal!.Choose(_nextDecision, select);
		return true;
	}

	public void Game_NewWaitingDecision(IDecision decision) {
		var gameState = GameState!;

		// Update Prompt
		_nextDecision = decision;
		Decision = _nextDecision;

		// Update Options
		Options = _nextDecision.Options.Select(o => new OptionModel(o)).ToArray();

		// Spirit
		SpiritSummary?.Update(_ovm);

		_ovm.SyncOptionStatesToDecision(decision);

		// Board
		if (ActionScope.Current == null)
			ActionScope.Initialize(gameState.RootScope);

		// Spaces = Spaces == "1" ? "0" : "1"; // IV.SyncTokensToGameState();
		Island.Sync();

		if (AutoSelect()) return;

		if (_nextDecision is A.GrowthDecision) ShowSpiritPanel();
		if (_nextDecision is A.PowerCard) ShowCardPanel();
	}

	public void ShowSpiritPanel(bool show = true) { if (SpiritPanel is not null) SpiritPanel.IsVisible = show; }
	public void ShowCardPanel(bool show = true) { if (Cards is not null) Cards.IsVisible = show; }


	public void ShutDownOld() {

		// !!! This is essentially a .Dispose() and ideally we don't want to need to call it.

		// If IslandView auto-disposed of its spaces tokens, we wouldn't have to do it here.
		Island.ClearIslandTokens();

		// release our event handlers
		_userPortal.NewWaitingDecision -= Game_NewWaitingDecision;
		_userPortal.Release();

		ShowCardPanel(false);
		ShowSpiritPanel(false);

		// Quit the game and wait a bit
		_game.UserPortal.Quit();
		if (!_game.EngineTask.Wait(500)) {
			// !!! log that the task did not complete
		}
	}

	public void CardsSelected(IOption[] options) { 
		_preSelectedOptions.UnionWith(options); 
		AutoSelect();
	}


#pragma warning disable IDE0290 // Use primary constructor
	public DecisionModel(GameState gameState) {
		if(ActionScope.Current == null)
			ActionScope.Initialize(gameState.RootScope);

		GameState = gameState;

		_ovm = new OptionViewManager();

		_gameStatus = new GameStatusModel(gameState.InvaderDeck, gameState.Fear, gameState.Tokens[SpiritIsland.BlightCard.Space]);

		// Setup the new game
		_game = new SinglePlayerGame(GameState) {
			LogExceptions = true,
			EnablePreselects = true,
		};

		// Wire up Game Notifications
		_userPortal = new UserPortalFacade(_game.UserPortal);
		_userPortal.NewWaitingDecision += Game_NewWaitingDecision;

		var gs = _game.GameState;
		GameStatus = new GameStatusModel(gs.InvaderDeck, gs.Fear, gs.Tokens[SpiritIsland.BlightCard.Space]);
		GameStatus.Watch(_userPortal, gs);
		SpiritSummary = new SpiritModel(_game.Spirit);

		// Cards
		Cards = new CardsModel(_game.Spirit, _userPortal);
		Cards.CardsSelected += CardsSelected;

		// Spirit Panel
		SpiritPanel?.Release();
		SpiritPanel = new SpiritPanelModel(_game.Spirit, _ovm, gs);

		_acceptText = "Accept";

		// when OVM gets a select/submit, update IDecision Model
		_ovm.OptionSelected += (option, submit) => {
			SelectedOption = option;
			if (submit)
				Submit();
		};

		// notify OVM that user Selected option changed.
		PropertyChanged += (sender, e) => {
			if (e.PropertyName == nameof(SelectedOption))
				_ovm.SelectedOption = ((DecisionModel)sender!).SelectedOption;
		};

		Island = new IslandModel(GameState.Island.Boards[0], GameState.Tokens, _ovm);
		_title = _game.Spirit.SpiritName;
	}

	public readonly OptionViewManager _ovm;
#pragma warning restore IDE0290 // Use primary constructor

	public void Watch(IUserPortal userPortal, GameState gs) {
		GameStatus.Watch(userPortal, gs);
	}

	public IDecision? _nextDecision;

	readonly internal UserPortalFacade _userPortal;
	readonly public GameState GameState;
	readonly public SinglePlayerGame _game;

	public readonly HashSet<IOption> _preSelectedOptions = [];

	public IslandModel Island {get;}


}

/// <summary> Wrapper to make Text visibile for explicitly defined .Text interface property </summary>
public class OptionModel(IOption option) {
	public IOption Option { get; } = option; 
	public string Text => Option.Text;
}