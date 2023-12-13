namespace SpiritIsland;

public class RemovingTokenArgs {

	public RemovingTokenArgs( SpaceState from, RemoveReason reason ) {
		From = from;
		Reason = reason;
	}

	// Read-only
	public SpaceState From { get; }
	public RemoveReason Reason { get; }

	// modifiable
	public IToken Token { get; set; }

	// Should never be negative.
	public int Count {
		get { return _count; }
		set { 
			if( value < 0 ) throw new ArgumentOutOfRangeException(nameof(Count),"Removing Count cannot be < 0");
			_count = value;
		}
	}
	int _count;
}