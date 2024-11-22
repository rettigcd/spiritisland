namespace SpiritIsland.Maui;

public class SpiritPanelModel : ObservableModel {

	public string SpiritName => _spirit.SpiritName;
	public SpecialRuleModel[] SpecialRules { get; }
	public InnateModel[] Innates { get; }

	public GrowthTrackModel GrowthTrack       { get => _growthTrack;       private set => SetProp(ref _growthTrack, value); }
	public TokenLocationModel[] EnergyTrack   { get => _energyTrack;       private set => SetProp(ref _energyTrack,value); }
	public TokenLocationModel[] CardPlayTrack { get => _cardTrack;         private set => SetProp(ref _cardTrack, value ); }
	public int Energy                         { get => _energy;            private set => SetProp(ref _energy, value); }
	public int Destroyed                      { get => _destroyed;         private set => SetProp(ref _destroyed, value); }
	public ElementDictModel Elements          { get => _elements;          private set => SetProp(ref _elements, value); }
	public ElementDictModel SecondaryElements { get => _secondaryElements; private set => SetProp(ref _secondaryElements, value); }

	public event Action? RequestClose;

	#region constructor

	public SpiritPanelModel( Spirit spirit, OptionViewManager ovm, GameState gsToWatch ) {

		_spirit = spirit;

		// add: Growth
		_growthTrack = new GrowthTrackModel(spirit.GrowthTrack);

		// Energy
		_energyTrack = spirit.Presence.Energy.Slots
			.Select(s => new TrackPresence(s, spirit.Presence.Token))
			.Select(tp => new TokenLocationModel(tp))
			.ToArray();

		// Cards
		_cardTrack = spirit.Presence.CardPlays.Slots
			.Select(s => new TrackPresence(s, spirit.Presence.Token))
			.Select(tp => new TokenLocationModel(tp))
			.ToArray();

		_energy = spirit.Energy;

		// Elements
		_secondaryElementHolder = _spirit as IHaveSecondaryElements;
		UpdateElements();

		// Innates
		Innates = [ ..spirit.InnatePowers.Select(ip=>new InnateModel(ip,spirit)) ];

		// Special Rules
		SpecialRules = spirit.SpecialRules.Select(x=>new SpecialRuleModel(x)).ToArray();

		// OVM
		ovm.AddRange(_energyTrack, this);
		ovm.AddRange(_cardTrack, this);
		ovm.AddRange(_growthTrack.Groups.SelectMany(grp => grp.Actions), this);

		gsToWatch.NewLogEntry += OnNewLogEntry;
		spirit.Portal.NewWaitingDecision += Portal_NewWaitingDecision; // using this to trigger UI update, NOT to get decision info.
	}

	void Portal_NewWaitingDecision(IDecision obj) {
		Energy = _spirit.Energy;
		Destroyed = _spirit.Presence.Destroyed.Count;
		UpdateElements();
		UpdateInnates();
	}

	#endregion constructor

	public void TryToClose() {
		RequestClose?.Invoke();
	}

	#region updates

	void OnNewLogEntry(Log.ILogEntry obj) {
		if( obj is TokenMovedArgs moved ) {
			if (moved.From is Track src)
				FindInTracks(src).RefreshCountAndSS();
			else if (moved.To is Track dst)
				FindInTracks(dst).RefreshCountAndSS();
		}
		if( obj is RewindException) {
			foreach(TokenLocationModel x in _energyTrack) 
				x.RefreshCountAndSS();
			foreach (TokenLocationModel x in _cardTrack) 
				x.RefreshCountAndSS();
		}
	}

	void UpdateElements() {
		Elements = new ElementDictModel(_spirit.Elements.Elements);
		if(_secondaryElementHolder is not null)
			SecondaryElements = new ElementDictModel( _secondaryElementHolder.SecondaryElements );
	}
	void UpdateInnates() {
		foreach(var innate in Innates)
			innate.Update();
	}

	TokenLocationModel FindInTracks(Track src) {
		return _energyTrack.FirstOrDefault(t => t.TokenLocation.Location == src)
			?? _cardTrack.First(t => t.TokenLocation.Location == src);
	}

	#endregion updates

	#region private fields

	GrowthTrackModel _growthTrack;
	TokenLocationModel[] _energyTrack;
	TokenLocationModel[] _cardTrack;
	int _energy;
	int _destroyed;
	ElementDictModel _elements = new ElementDictModel([]);
	ElementDictModel _secondaryElements = new ElementDictModel([]);

	readonly IHaveSecondaryElements? _secondaryElementHolder;
	readonly Spirit _spirit;

	#endregion
}
