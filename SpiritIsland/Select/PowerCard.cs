namespace SpiritIsland.A;

using Type = SpiritIsland.PowerCard; // avoid name conflict

/// <summary>
/// Selects a PowerCard from a group of PowerCard options.
/// </summary>
public class PowerCard : TypedDecision<Type> {

	#region constructors

	public PowerCard(string prompt, int numberToSelect, CardUse cardUse, Type[] cardOptions, Present present ) 
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
	public CardUse Use( Type card ) => _cardUses[card];

	public Type[] CardOptions => _cardUses.Keys.ToArray();

	readonly Dictionary<Type, CardUse> _cardUses = [];

}

