namespace SpiritIsland;

public class RemovingTokenArgs {

	public RemovingTokenArgs( SpaceState space, RemoveReason reason, UnitOfWork actionScope ) {
		Space = space;
		Reason = reason;
		ActionScope = actionScope;
	}

	// Read-only
	public SpaceState Space { get; }
	public RemoveReason Reason { get; }
	public UnitOfWork ActionScope { get; }

	// modifiable
	public Token Token { get; set; }
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
