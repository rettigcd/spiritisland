namespace SpiritIsland.A;

public class MoveDecision( string prompt, IEnumerable<SpiritIsland.Move> options, Present present ) 
	: TypedDecision<Move>( prompt, options, present ) 
{}

