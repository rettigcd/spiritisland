namespace SpiritIsland;

public interface IHandleRemovingToken {
	/// <summary>
	/// Called for both stopping a remove and for testing if we can stop a remove.
	/// </summary>
	Task ModifyRemoving( RemovingTokenArgs args );
}
