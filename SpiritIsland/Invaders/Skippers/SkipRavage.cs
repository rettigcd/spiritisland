namespace SpiritIsland;

/// <summary> Stops 1 Ravage. </summary>
public class SkipRavage : SelfCleaningToken, ISkipRavages {

	public SkipRavage() : base() { }

	public virtual Task<bool> Skip( GameState _, SpaceState space ) {
		space.Adjust( this, -1 ); // remove this token
		return Task.FromResult( true ); // stopped
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

}

