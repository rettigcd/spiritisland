namespace SpiritIsland;

/// <summary> Stops either 1 or ALL explores </summary>
public class SkipExploreTo : SelfCleaningToken, ISkipExploreTo {

	public SkipExploreTo():base() { }

	public virtual Task<bool> Skip( GameCtx _, SpaceState space ) {
		space.Adjust( this, -1 ); // remove this token
		return Task.FromResult( true ); // stopped
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

}
