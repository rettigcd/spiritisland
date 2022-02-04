namespace SpiritIsland.Select;

using Type = SpiritIsland.PowerCard; // avoid name conflict

public class PowerCard : TypedDecision<Type> {

	public CardUse Use(Type card) => cardUses[card];
	readonly Dictionary<Type,CardUse> cardUses = new Dictionary<Type, CardUse>();

	public PowerCard(string prompt, CardUse cardUse, Type[] cardOptions, Present present ) 
		: base( prompt, cardOptions, present
	) {
		foreach(var option in cardOptions)
			cardUses.Add(option,cardUse);
	}

	public PowerCard(string prompt, IEnumerable<SingleCardUse> cardOptions, Present present ) 
		: base( prompt, cardOptions.Select(x=>x.Card), present
	) {
		foreach(var option in cardOptions)
			cardUses.Add(option.Card,option.Use);
	}

	public Type[] CardOptions => cardUses.Keys.ToArray();

}