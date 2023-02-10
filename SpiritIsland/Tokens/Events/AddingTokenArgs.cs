namespace SpiritIsland;

public class AddingTokenArgs {

	public AddingTokenArgs(SpaceState spaceState, AddReason addReason ) {
		Space = spaceState;
		Reason = addReason;
	}

	public SpaceState Space { get; }
	public AddReason Reason { get; }

	// Modifiable
	public IToken Token { get; set; }
	public int Count { get; set; }
}
