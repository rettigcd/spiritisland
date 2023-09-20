namespace SpiritIsland.Select;

using Type = SpiritIsland.PowerCard; // avoid name conflict

/// <summary>
/// Selects a PowerCard from a group of PowerCard options.
/// </summary>
public class PowerCard : TypedDecision<Type> {

	#region constructors

	public PowerCard(string prompt, CardUse cardUse, Type[] cardOptions, Present present ) 
		: base( prompt, cardOptions, present
	) {
		foreach(var option in cardOptions)
			_cardUses.Add(option,cardUse);
	}

	public PowerCard(string prompt, IEnumerable<SingleCardUse> cardOptions, Present present ) 
		: base( prompt, cardOptions.Select(x=>x.Card), present
	) {
		foreach(var option in cardOptions)
			_cardUses.Add(option.Card,option.Use);
	}

	#endregion

	/// <summary> If we select the given PowerCard, what are we going to do with it.  What is its use? </summary>
	public CardUse Use( Type card ) => _cardUses[card];

	public Type[] CardOptions => _cardUses.Keys.ToArray();

	readonly Dictionary<Type, CardUse> _cardUses = new Dictionary<Type, CardUse>();

}