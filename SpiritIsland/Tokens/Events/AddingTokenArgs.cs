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
	public IVisibleToken Token { get; set; }
	public int Count { get; set; }
}
