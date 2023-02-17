namespace SpiritIsland;

public interface IModifyRemovingTokenAsync {
	/// <summary>
	/// Called for both stopping a remove and for testing if we can stop a remove.
	/// </summary>
	Task ModifyRemovingAsync( RemovingTokenArgs args );
}

public interface IModifyRemovingToken {
	/// <summary>
	/// Called for both stopping a remove and for testing if we can stop a remove.
	/// </summary>
	void ModifyRemoving( RemovingTokenArgs args );
}
