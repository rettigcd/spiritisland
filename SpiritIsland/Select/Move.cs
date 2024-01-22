namespace SpiritIsland.A;

public class Move( string prompt, IEnumerable<SpiritIsland.Move> options, Present present ) 
	: TypedDecision<SpiritIsland.Move>( prompt, options, present ) 
{}

