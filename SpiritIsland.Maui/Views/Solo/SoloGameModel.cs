using SpiritIsland.SinglePlayer;
using System.Windows.Input;

namespace SpiritIsland.Maui;

public class SoloGameModel : ObservableModel {

	// * NOT USED - but required to Make child models compile
	public bool IsVisible {get; set;}

	#region Models for child Views

	public GameState GameState          { get; }
	public IslandModel Island           { get; }
	public SpiritSummaryModel SpiritSummary    { get; } // binding for summary panel
	public SpiritPanelModel SpiritPanel { get; } // binding for spirit panel
	public CardsOverlayModel Cards      { get; }
	public LogModel Log                 { get; }

	public ICommand ShowCards           { get; }
	public ICommand OpenOverlay         { get; }
	public string Adversary             { get; }
	public bool HasAdversary => !string.IsNullOrWhiteSpace(Adversary);

	#endregion Models for child Views

	#region Rewind / Phase - Observable

	public ICommand RewindCommand { get; }
	public int RewindableRound { get => _rewindableRound; set => SetProp(ref _rewindableRound, value); }
	public Phase Phase { get => _phase; set { SetProp(ref _phase, value); } }
	int _rewindableRound;
	Phase _phase;

	#endregion Rewind / Phase - Observable

	#region Observable properties

	/// <summary> Indicates which overlay panel is visible (if any) </summary>
	public Overlay VisibleOverlay       { get => _visibleOverlay; set => SetProp( ref _visibleOverlay, value ); }

	/// <summary> Appears in the Title section of the page.  Should contain: spirit, game #, Adversary </summary>
	public string Title                 { get => _title; set => SetProp(ref _title,value); }
	public InvaderBoardModel InvaderBoard { get => _gameStatus; set => SetProp(ref _gameStatus, value); }

	/// <summary> Decision Prompt </summary>
	public string Prompt                { get => _prompt; set => SetProp(ref _prompt, value); }
	/// <summary> Current selected Option - the one that can be 'Accept'ed </summary>
	public IOption? SelectedOption      { get => _option; set {
			SetProp( ref _option, value );
			if(value is not null)
				UpdateButton(((IOption)value).Text, true);
			else
				UpdateButton("Accept", false);
		} }
	/// <summary> Decision Options </summary>
	public OptionModel[] Options        { get => _options; private set => SetProp(ref _options, value); }
	public bool HasOptionReady          { get => _hasOptionReady; set => SetProp(ref _hasOptionReady, value); }
	public string AcceptText            { get => _acceptText; set => SetProp(ref _acceptText, value); }

	void UpdateButton(string buttonText, bool hasOptionReady) {
		AcceptText = buttonText;
		HasOptionReady = hasOptionReady;
	}

	#endregion

	#region constructor

	public SoloGameModel(GameState gameState) {
		if (ActionScope.Current == null)
			ActionScope.Initialize(gameState.RootScope);

		GameState = gameState;

		_ovm = new OptionViewManager();

		// Setup the new game
		_game = new SinglePlayerGame(GameState) {
			LogExceptions = true,
			EnablePreselects = true,
		};

		// Wire up Game Notifications
		_userPortal = new UserPortalFacade(_game.UserPortal.DecisionPortal);
		_userPortal.NewWaitingDecision += Game_NewWaitingDecision;

		var gs = _game.GameState;
		_gameStatus = new InvaderBoardModel( _game );

		// Cards
		Cards = new CardsOverlayModel(_game.Spirit, _userPortal);
		Cards.CardsSelected += CardsSelected;
		Cards.RequestClose += Overlay_RequestClose;

		// Spirit Panel
		SpiritSummary = new SpiritSummaryModel(_game.Spirit, new Command(()=>VisibleOverlay=Overlay.SpiritPanel));
		SpiritPanel = new SpiritPanelModel(_game.Spirit, _ovm, gs);
		SpiritPanel.RequestClose += Overlay_RequestClose;

		Adversary = gs.Adversary!.Name;

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
				_ovm.SelectedOption = ((SoloGameModel)sender!).SelectedOption;
		};

		Island = new IslandModel(GameState, _ovm);
		_title = _game.Spirit.SpiritName;

		_phase = gs.Phase;

		gs.NewLogEntry += Gs_NewLogEntry;

		Log = new LogModel();
		gs.NewLogEntry += Log.Gs_NewLogEntry;

		ShowCards = new Command( () => ShowCardPanel() );
		OpenOverlay = new Command((p) => { VisibleOverlay = (Overlay)p; } );
		RewindCommand = new Command( DoRewind );

	}

	void Gs_NewLogEntry(Log.ILogEntry obj) {
		if (obj is Log.Phase phaseEntry)
			Phase = phaseEntry.phase;
	}

	void DoRewind() {
		if(_game.GameState.Result is not null) return;
		if (0 < RewindableRound) {
			_game.UserPortal.RewindToRound(RewindableRound);
			--RewindableRound;
		}
	}

	#endregion constructor

	public void Start() => _game.Start();

	/// <summary> Submits the currently selected Option as the Decisions choice. </summary>
	public void Submit() {
		if (_nextDecision is null) { AcceptText = "-- missing decision --"; return; }
		if (SelectedOption is null) { AcceptText = "-no selection --"; return; }

		UpdateButton("working...", false);

		_userPortal.Choose(_nextDecision, (IOption)SelectedOption);
		RewindableRound = _game.GameState.RoundNumber;
	}

	public bool AutoSelect() {
		IOption? select = _nextDecision!.Options.FirstOrDefault(_preSelectedOptions.Contains);
		if (select is null) return false;
		_preSelectedOptions.Remove(select);
		_userPortal!.Choose(_nextDecision, select);
		return true;
	}

	// Overlays 

	public void ShowSpiritPanel(bool show = true) { 
		VisibleOverlay = show ? Overlay.SpiritPanel : Overlay.None;
	}

	public void ShowCardPanel(bool show = true) { 
		VisibleOverlay = show ? Overlay.Cards : Overlay.None;
	}

	void Overlay_RequestClose() {
		VisibleOverlay = Overlay.None;
	}

	/// <summary> Game is over, Dispose / Release everythins </summary>
	public void ShutDown() {

		// !!! This is essentially a .Dispose() and ideally we don't want to need to call it.

		// If IslandView auto-disposed of its spaces tokens, we wouldn't have to do it here.
		Island.ClearIslandTokens();

		// release our event handlers
		_userPortal.NewWaitingDecision -= Game_NewWaitingDecision;
		_userPortal.Release();

		ShowCardPanel(false);
		ShowSpiritPanel(false);

		// Quit the game and wait a bit
		_game.UserPortal.CancelGame();
		if (!_game.EngineTask!.Wait(500)) {
			// !!! log that the task did not complete
		}
	}

	void Game_NewWaitingDecision(IDecision decision) {

		// Update Prompt
		_nextDecision = decision;
		Prompt = _nextDecision.Prompt;
		SelectedOption = null;

		// Update Options
		Options = []; // On Android, it seems to be re-using radio buttons that are already selected.  So clear it out so it doesn't reuse a checked radio button.
		Options = _nextDecision.Options.Select(o => new OptionModel(o)).ToArray();

		// Spirit
		SpiritSummary?.Update(_ovm);

		_ovm.SyncOptionStatesToDecision(decision);

		// Board
		if (ActionScope.Current == null)
			ActionScope.Initialize(GameState.RootScope);

		// Spaces = Spaces == "1" ? "0" : "1"; // IV.SyncTokensToGameState();
		Island.SyncTokens();

		if (AutoSelect()) return;

		if (_nextDecision is A.GrowthDecision) ShowSpiritPanel();
		if (_nextDecision is A.PowerCard) ShowCardPanel();
	}

	void CardsSelected(IOption[] options) {
		_preSelectedOptions.UnionWith(options);
		AutoSelect();
	}

	InvaderBoardModel _gameStatus;

	string _prompt = string.Empty;
	OptionModel[] _options = [];
	IOption? _option;
	bool _hasOptionReady;
	string _acceptText;
	string _title = "";
	IDecision? _nextDecision;
	Overlay _visibleOverlay = Overlay.None;

	readonly SinglePlayerGame _game;
	readonly internal UserPortalFacade _userPortal;
	readonly HashSet<IOption> _preSelectedOptions = [];
	readonly OptionViewManager _ovm;
}

public enum Overlay { None, Cards, SpiritPanel, InvaderBoard }