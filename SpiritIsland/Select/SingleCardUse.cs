namespace SpiritIsland;

public class SingleCardUse {
	public CardUse Use { get; set; }
	public PowerCard Card { get; set; }
	static public IEnumerable<SingleCardUse> GenerateUses(CardUse use, IEnumerable<PowerCard> cards ) {
		foreach(var card in cards)
			yield return new SingleCardUse {  Card = card, Use = use };
	}

}