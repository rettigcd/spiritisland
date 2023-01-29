namespace SpiritIsland;

public class RemovingTokenArgs {

	public RemovingTokenArgs( ActionableSpaceState space, RemoveReason reason ) {
		Space = space;
		Reason = reason;
	}

	// Read-only
	public ActionableSpaceState Space { get; }
	public RemoveReason Reason { get; }
	public UnitOfWork ActionScope => Space.ActionScope;

	// modifiable
	public Token Token { get; set; } // !!! Should this be IVisibleToken?   
	// !!! Should all Add/Remove/Destroy tokens take IVisibleTokens only ???

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
