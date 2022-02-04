namespace SpiritIsland.Select;

public class DeckToDrawFrom : TypedDecision<PowerType> {
	public DeckToDrawFrom(params PowerType[] types):base("Which type do you wish to draw", types ) { }
}
