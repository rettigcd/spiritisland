
using System.Windows.Input;

namespace SpiritIsland.Maui;

public class SpiritModel : ObservableModel {

	#region Observable Properties

	public string Spirit { get; }
	public int Energy                         { get => _energy;           set => SetProp( ref _energy, value);           } int _energy;
	public int EnergyPerTurn                  { get => _energyPerTurn;    set => SetProp( ref _energyPerTurn, value);    } int _energyPerTurn;
	public int CardPlaysPerTurn               { get => _cardPlaysPerTurn; set => SetProp( ref _cardPlaysPerTurn, value); } int _cardPlaysPerTurn;
	public ElementDictModel Elements          { get => _elements;         set => SetProp( ref _elements, value);         } ElementDictModel _elements = new ElementDictModel([]);
	public ElementDictModel SecondaryElements { get => _secondaryElements;set => SetProp( ref _secondaryElements, value);} ElementDictModel _secondaryElements = new ElementDictModel([]);
	public TokenLocationModel[] TrackPresence { get => _trackPresence;    set => SetProp( ref _trackPresence, value);    } TokenLocationModel[] _trackPresence = [];

	#endregion

	public ICommand OpenSpiritPanel { get; }

	#region constructor

	public SpiritModel(Spirit spirit, ICommand openSpiritPanel) {
		_spirit = spirit;

		Spirit = spirit.SpiritName;
		_energy = _spirit.Energy;
		_energyPerTurn = _spirit.EnergyPerTurn;
		_cardPlaysPerTurn = _spirit.NumberOfCardsPlayablePerTurn;
		OpenSpiritPanel = openSpiritPanel;
	}

	#endregion constructor

	public void Update(OptionViewManager ovm) {
		// Update properties to trigger notification
		Energy = _spirit.Energy;
		EnergyPerTurn = _spirit.EnergyPerTurn;
		CardPlaysPerTurn = _spirit.NumberOfCardsPlayablePerTurn;
		Elements = new ElementDictModel(_spirit.Elements.Elements);
		if(_spirit is IHaveSecondaryElements ihse)
			SecondaryElements = new ElementDictModel(ihse.SecondaryElements);

		// Track Presence to Reveal
		var revealed = _spirit.Presence.RevealOptions().ToArray();
		if( !revealed.SequenceEqual(_oldRevealed)) {
			ovm.RemoveRange(TrackPresence); // remove old
			TrackPresence = revealed.Select(t => new TokenLocationModel(t)).ToArray();
			ovm.AddRange( TrackPresence ); // add new
			_oldRevealed = revealed;
		}
	}


	TokenLocation[] _oldRevealed = [];

	readonly Spirit _spirit;
}
