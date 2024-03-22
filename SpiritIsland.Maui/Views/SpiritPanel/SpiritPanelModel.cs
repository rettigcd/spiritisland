namespace SpiritIsland.Maui;

public class SpiritPanelModel : ObservableModel {

	public string SpiritName => _spirit.SpiritName;
	public SpecialRuleModel[] SpecialRules { get; }
	public InnateModel[] Innates { get; }

	public bool IsVisible {
		get => _isVisible;
		set => SetProp(ref _isVisible, value);
	}
	public GrowthTrackModel GrowthTrack       { get => _growthTrack; private set => SetProp(ref _growthTrack, value); }
	public TokenLocationModel[] EnergyTrack   { get => _energyTrack;       private set => SetProp(ref _energyTrack,value); }
	public TokenLocationModel[] CardPlayTrack { get => _cardTrack;         private set => SetProp(ref _cardTrack, value ); }
	public ElementDictModel Elements          { get => _elements;          private set => SetProp(ref _elements, value); }
	public ElementDictModel SecondaryElements { get => _secondaryElements; private set => SetProp(ref _secondaryElements, value); }

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

		// Elements
		_secondaryElementHolder = _spirit as IHaveSecondaryElements;
		UpdateElements();

		// Innates
		Innates = [ ..spirit.InnatePowers.Select(ip=>new InnateModel(ip)) ];

		// Special Rules
		SpecialRules = spirit.SpecialRules.Select(x=>new SpecialRuleModel(x)).ToArray();


		// OVM
		ovm.AddRange(_energyTrack, this);
		ovm.AddRange(_cardTrack, this);
		ovm.AddRange(_growthTrack.Groups.SelectMany(grp => grp.Actions), this);
		_ovm = ovm;

		gsToWatch.NewLogEntry += OnNewLogEntry;
	}

	#endregion constructor

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
		UpdateElements();
	}

	void UpdateElements() {
		Elements = new ElementDictModel(_spirit.Elements.Elements);
		if(_secondaryElementHolder is not null)
			SecondaryElements = new ElementDictModel( _secondaryElementHolder.SecondaryElements );
	}

	TokenLocationModel FindInTracks(Track src) {
		return _energyTrack.FirstOrDefault(t => t.TokenLocation.Location == src)
			?? _cardTrack.First(t => t.TokenLocation.Location == src);
	}

	#endregion updates

	#region private fields

	bool _isVisible;
	GrowthTrackModel _growthTrack;
	TokenLocationModel[] _energyTrack;
	TokenLocationModel[] _cardTrack;
	ElementDictModel _elements = new ElementDictModel([]);
	ElementDictModel _secondaryElements = new ElementDictModel([]);

	readonly IHaveSecondaryElements? _secondaryElementHolder;
	readonly OptionViewManager _ovm;
	readonly Spirit _spirit;

	#endregion
}
