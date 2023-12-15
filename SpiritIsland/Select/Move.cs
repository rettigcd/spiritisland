namespace SpiritIsland.A;

public class Move : TypedDecision<SpiritIsland.Move> {
	public Move(string prompt, IEnumerable<SpiritIsland.Move> options, Present present ) : base( prompt, options, present ) { }
}

