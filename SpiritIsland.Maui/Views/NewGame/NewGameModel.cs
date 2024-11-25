using System.Collections.ObjectModel;

namespace SpiritIsland.Maui;

// Model
class NewGameModel : ObservableModel {

	// Spirit
	public ObservableCollection<NamedModel> RecentSpirits { get; }
	public NamedModel[] AvailableSpirits { get; }
	public string? SelectedSpirit { get => _selectedSpirit; set => SetProp(ref _selectedSpirit, value); }
	public Command EditSpirit { get; }
	public Command EditSpiritCancel { get; }
	public bool IsEditingSpirit { get => _isEditingSpirit; set => SetProp(ref _isEditingSpirit, value); }

	// Adversary
	public NamedModel[] AvailableAdversaries { get; }
	public AdversaryModel SelectedAdversary { get => _selectedAdversary; set => SetProp(ref _selectedAdversary, value); }
	public AdversaryModel? FocusAdversary { 
		get => _focusAdversary; 
		set { 
			SetProp(ref _focusAdversary, value);
			HasFocusAdversary = value is not null;
		}
	}
	public Command EditAdversary { get; }
	public Command EditAdversaryAccept { get; }
	public Command EditAdversaryCancel { get; }
	public Command NoAdversary { get; }
	/// <summary> controls Adversary Panel visibility. </summary>
	public bool HasFocusAdversary { get => _hasFocusAdversary; private set => SetProp(ref _hasFocusAdversary, value); }

	// Boards
	public string[] AvailalbeBoards { get; } = ["A", "B", "C", "D", "E", "F"];
	public string? SelectedBoard { get => _selectedBoard; set=>SetProp(ref _selectedBoard,value); }

	// Game #
	public string GameNumber { get => _gameNumber; set => SetProp(ref _gameNumber, value); }
	string _gameNumber = "";

	public bool CanStart { get => _canStart; set => SetProp(ref _canStart, value); }
	public Command CreateGame { get; }

	#region constructors

	public NewGameModel() {

		// Spirits
		AvailableSpirits = _builder.SpiritNames.Select(ConstructSpirit).ToArray();

		RecentSpirits = new( SavedRecentSpirits.Select(ConstructSpirit) );
		EditSpirit = new Command( DoEditSpirit );
		EditSpiritCancel = new Command(DoEditSpiritCancel);

		// Adversaries
		AvailableAdversaries = _builder.AdversaryNames.Select(ConstructFlagModel).ToArray();
		_selectedAdversary = new AdversaryModel( _builder.GetAdversaryBuilder("") );
		EditAdversary = new Command(DoEditAdversary);
		EditAdversaryAccept = new Command(DoEditAdversaryAccept);
		EditAdversaryCancel = new Command(DoEditAdversaryCancel);
		NoAdversary = new Command(DoNoAdversary);

		// Beast
		CommandBeast = true;

		// Start
		CreateGame = new Command(DoStart);
	}

	NamedModel ConstructSpirit(string spiritName) {
		var model = new NamedModel(spiritName);
		model.RequestSelected += SelectSpirit;
		return model;
	}

	NamedModel ConstructFlagModel(string name) {
		var model = new NamedModel(name);
		model.RequestSelected += AdversarySelected;
		return model;
	}

	#endregion constructors

	#region Spirit methods

	void SelectSpirit(NamedModel spiritNameModel) {
		SelectedSpirit = spiritNameModel.Name;
		IsEditingSpirit = false;
		CanStart = true;
	}

	void DoEditSpirit() { IsEditingSpirit = true; }
	void DoEditSpiritCancel() { IsEditingSpirit = false; }

	public void SaveSelectedSpiritAsRecent() {
		string spirit = SelectedSpirit!;
		// Update Recents Collection
		var recentSpiritModel = RecentSpirits.FirstOrDefault(x=>x.Name == spirit);
		if(recentSpiritModel is not null)
			RecentSpirits.Remove(recentSpiritModel);
		else
			recentSpiritModel = ConstructSpirit(spirit);
		RecentSpirits.Insert(0, recentSpiritModel);
		// Save
		SavedRecentSpirits = [.. RecentSpirits.Select(x=>x.Name)];
	}

	/// <summary> Saves/loads recent spirits from preferences. </summary>
	static string[] SavedRecentSpirits {
		get => [.. Preferences.Default.Get(RecentSpiritsKey, "").Split(",").Where(s => !string.IsNullOrEmpty(s))];
		set => Preferences.Default.Set(RecentSpiritsKey, value.Join(","));
	}
	const string RecentSpiritsKey = "RecentSpirits";

	#endregion Spirit methods

	#region Adversary methods

	// Updates flags and sets FocusAdversary
	void AdversarySelected(NamedModel selectedAdversary) {
		HighlightFlag(selectedAdversary.Name);
		FocusAdversary = new AdversaryModel( _builder.GetAdversaryBuilder(selectedAdversary.Name) );
	}
	void HighlightFlag(string name) {
		foreach( var aa in AvailableAdversaries ) aa.IsActive = aa.Name == name;
	}

	void DoEditAdversary() {
		FocusAdversary = SelectedAdversary;
		HighlightFlag(FocusAdversary.Name);
	}
	void DoEditAdversaryAccept() {
		if(SelectedAdversary == FocusAdversary)
			Notify(nameof(SelectedAdversary)); // Update Level even if item didn't change
		SelectedAdversary = FocusAdversary!;
		FocusAdversary = null;
	}
	void DoEditAdversaryCancel() {
		FocusAdversary = null;
	}
	void DoNoAdversary() {
		SelectedAdversary = new AdversaryModel(_builder.GetAdversaryBuilder(""));
		FocusAdversary = null;
	}

	#endregion private Adversary methods

	public bool CommandBeast { get=>_commandBeast; set=>SetProp(ref _commandBeast, value); }

	#region Start
	void DoStart() {
		// Spirit
		SaveSelectedSpiritAsRecent();

		// Get Board (or randomize)
		string? board = SelectedBoard;
		if( string.IsNullOrEmpty(board) )
			board = AvailalbeBoards[(int)(DateTime.Now.Ticks % AvailalbeBoards.Length)];

		// Init Configuration
		var advConfig = SelectedAdversary.ToConfig();
		var gc = new GameConfiguration()
			.ConfigSpirits(SelectedSpirit!)
			.ConfigBoards(board)
			.ConfigCommandBeasts(CommandBeast) 
			.ConfigAdversary(advConfig);
		gc.ShuffleNumber = !string.IsNullOrWhiteSpace(GameNumber) ? int.Parse(GameNumber) : (int)DateTime.Now.Ticks;

		LastConfig = $"Spirit: {SelectedSpirit!} Board:{board} G#:{gc.ShuffleNumber} Adv:{advConfig.Name}-{advConfig.Level}";

		var gameState = _builder.BuildGame(gc);
		NewGameCreated?.Invoke(gameState);
	}
	public string LastConfig = "";
	public event Action<GameState>? NewGameCreated;
	#endregion Start

	#region private fields

	// Observable backing field
	string? _selectedSpirit;
	bool _isEditingSpirit;
	AdversaryModel _selectedAdversary;
	AdversaryModel? _focusAdversary;
	bool _hasFocusAdversary;
	bool _canStart;
	string? _selectedBoard;
	bool _commandBeast;

	readonly GameBuilder _builder = new GameBuilder(
		new Basegame.GameComponentProvider(),
		new BranchAndClaw.GameComponentProvider(),
		new FeatherAndFlame.GameComponentProvider(),
		new JaggedEarth.GameComponentProvider(),
		new NatureIncarnate.GameComponentProvider()
	);

	#endregion private fields
}
