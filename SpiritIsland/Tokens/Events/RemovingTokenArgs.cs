namespace SpiritIsland;

public class RemovingTokenArgs {

	public RemovingTokenArgs( SpaceState from, RemoveReason reason, RemoveMode mode ) {
		From = from;
		Reason = reason;
		Mode = mode;
	}

	// Read-only
	public SpaceState From { get; }
	public RemoveReason Reason { get; }

	// Test Remove / Live Remove
	public RemoveMode Mode { get; }

	// modifiable
	public IToken Token { get; set; }

	// Should never be negative.
	public int Count { get; set; }
}

public enum RemoveMode {
	/// <summary> Actually performing a remove.</summary>
	Live, 
	/// <summary> Premptively Testing if a token can be removed.</summary>
	Test
}