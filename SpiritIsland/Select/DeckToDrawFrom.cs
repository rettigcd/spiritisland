namespace SpiritIsland.A;

public class DeckToDrawFrom( params PowerType[] types ) 
	: TypedDecision<PowerType>("Which type do you wish to draw", types ) 
{
	public PowerType[] PowerTypes { get; } = types;
}
