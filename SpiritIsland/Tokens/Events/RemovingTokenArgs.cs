namespace SpiritIsland;

public class RemovingTokenArgs {

	public RemovingTokenArgs( SpaceState space, RemoveReason reason, UnitOfWork uow ) {
		Space = space;
		Reason = reason;
		UnitOfWork = uow;
	}

	// Read-only
	public Token Token { get; set; }
	public SpaceState Space { get; }
	public RemoveReason Reason { get; }
	public UnitOfWork UnitOfWork { get; }

	// modifiable
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
