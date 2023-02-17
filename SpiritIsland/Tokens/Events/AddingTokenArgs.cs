namespace SpiritIsland;

public class AddingTokenArgs {

	public AddingTokenArgs(SpaceState to, AddReason addReason ) {
		To = to;
		Reason = addReason;
	}

	public SpaceState To { get; }
	public AddReason Reason { get; }

	// Modifiable
	public IToken Token { get; set; }
	public int Count { get; set; }
}
