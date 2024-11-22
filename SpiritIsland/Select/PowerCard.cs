namespace SpiritIsland.A;

using SI_PowerCard = SpiritIsland.PowerCard; // avoid name conflict

/// <summary>
/// Selects a PowerCard from a group of PowerCard options.
/// </summary>
public class PowerCard : TypedDecision<SI_PowerCard> {

	#region constructors

	public PowerCard(string prompt, int numberToSelect, CardUse cardUse, SI_PowerCard[] cardOptions, Present present ) 
		: base( prompt, cardOptions, present )
	{
		foreach(var option in cardOptions)
			_cardUses.Add(option,cardUse);
		NumberToSelect = numberToSelect;
	}

	/// <summary>
	/// Selects 1 Power Card at a time.
	/// </summary>
	public PowerCard(string prompt, IEnumerable<SingleCardUse> cardOptions, Present present ) 
		: base( prompt, cardOptions.Select(x=>x.Card), present
	) {
		foreach(var option in cardOptions)
			_cardUses.Add(option.Card,option.Use);
		NumberToSelect = 1;
	}

	#endregion

	public int NumberToSelect {get;}

	/// <summary> If we select the given PowerCard, what are we going to do with it.  What is its use? </summary>
	public CardUse Use( SI_PowerCard card ) => _cardUses[card];

	public SI_PowerCard[] CardOptions => [.. _cardUses.Keys];

	readonly Dictionary<SI_PowerCard, CardUse> _cardUses = [];

}

