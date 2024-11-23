using System.Collections.ObjectModel;

namespace SpiritIsland.Maui;

// Model
class NewGameModel : ObservableModel {

	// Spirit
	public ObservableCollection<string> RecentSpirits { get; }
	public string[] AvailableSpirits { get; }
	public string? SelectedSpirit { get => _selectedSpirit; set => SetProp(ref _selectedSpirit, value); }

	// Adversary
	public AdversaryFlagModel[] AvailableAdversaries { get; }
	public AdversaryModel SelectedAdversary { get => _selectedAdversary; set => SetProp(ref _selectedAdversary, value); }
	public AdversaryModel? FocusAdversary { 
		get => _focusAdversary; 
		set { 
			SetProp(ref _focusAdversary, value);
			HasFocusAdversary = value is not null;
		}
	}
	public bool HasFocusAdversary { get => _hasFocusAdversary; private set => SetProp(ref _hasFocusAdversary, value); }
	public Command EditAdversary { get; }
	public Command EditAdversaryAccept { get; }
	public Command EditAdversaryCancel { get; }
	public Command NoAdversary { get; }

	#region constructors

	public NewGameModel(GameBuilder builder) {
		_builder = builder;

		// Spirits
		AvailableSpirits = builder.SpiritNames;
		RecentSpirits = new( SavedRecentSpirits );
		RecentSpirits.CollectionChanged += (s,c) => SavedRecentSpirits = [.. RecentSpirits];

		// Adversaries
		AvailableAdversaries = builder.AdversaryNames.Select(ConstructFlagModel).ToArray();
		_selectedAdversary = new AdversaryModel( builder.GetAdversaryBuilder("") );
		EditAdversary = new Command(DoEditAdversary);
		EditAdversaryAccept = new Command(DoEditAdversaryAccept);
		EditAdversaryCancel = new Command(DoEditAdversaryCancel);
		NoAdversary = new Command(DoNoAdversary);
	}

	AdversaryFlagModel ConstructFlagModel(string name) {
		var model = new AdversaryFlagModel(name);
		model.RequestSelected += AdversarySelected;
		return model;
	}

	#endregion constructors

	#region private Adversary methods

	// Updates flags and sets FocusAdversary
	void AdversarySelected(AdversaryFlagModel selectedAdversary) {
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


	// Available Boards
	// Selected Board
	// Action: Select Boards

	// Selected Game #.
	// Action: set game #

	// StartGame

	public void SaveSelectedSpiritAsRecent() {
		var spirit = SelectedSpirit!;
		RecentSpirits.Remove(spirit);
		RecentSpirits.Insert(0, spirit);
	}

	static string[] SavedRecentSpirits {
		get => [.. Preferences.Default.Get(RecentSpiritsKey, "").Split(",").Where(s => !string.IsNullOrEmpty(s))];
		set => Preferences.Default.Set(RecentSpiritsKey, value.Join(","));
	}
	const string RecentSpiritsKey = "RecentSpirits";

	readonly GameBuilder _builder;

	// Observable backing field
	string? _selectedSpirit;
	AdversaryModel _selectedAdversary;
	AdversaryModel? _focusAdversary;
	bool _hasFocusAdversary;

}
