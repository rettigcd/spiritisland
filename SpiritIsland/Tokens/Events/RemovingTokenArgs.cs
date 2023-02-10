namespace SpiritIsland;

public class RemovingTokenArgs {

	public RemovingTokenArgs( SpaceState space, RemoveReason reason ) {
		Space = space;
		Reason = reason;
	}

	// Read-only
	public SpaceState Space { get; }
	public RemoveReason Reason { get; }

	// modifiable
	public IToken Token { get; set; }

	public int Count {
		get { return _count; }
		set { 
			// !!! something is making this negative
			if(value<0) throw new ArgumentOutOfRangeException(nameof(value),value,"Removing Token Args cannot be < 0");
			_count = value;
		}
	}
	int _count;
}
