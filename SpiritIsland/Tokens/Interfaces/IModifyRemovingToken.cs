namespace SpiritIsland;

public interface IModifyRemovingToken {
	/// <summary>
	/// Called for both stopping a remove and for testing if we can stop a remove.
	/// </summary>
	Task ModifyRemovingAsync( RemovingTokenArgs args );
}
