namespace SpiritIsland;

public class AddingTokenArgs {

	public AddingTokenArgs(SpaceState spaceState, AddReason addReason, UnitOfWork actionScope ) {
		Space = spaceState;
		Reason = addReason;
		ActionScope = actionScope;
	}

	public SpaceState Space { get; }
	public AddReason Reason { get; }
	public UnitOfWork ActionScope { get; }

	// Modifiable
	public Token Token { get; set; }
	public int Count {
		get { return _count; }
		set {
			// !!! something is making this negative
			if(value < 0) throw new ArgumentOutOfRangeException( nameof( value ), value, "Removing Token Args cannot be < 0" );
			_count = value;
		}
	}
	int _count;
}
