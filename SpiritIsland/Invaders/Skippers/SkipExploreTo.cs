namespace SpiritIsland;

/// <summary> Stops either 1 or ALL explores </summary>
public class SkipExploreTo( bool skipAll = false ) : BaseModEntity(), IEndWhenTimePasses, ISkipExploreTo {
	public virtual Task<bool> Skip( SpaceState space ) {
		if(!skipAll)
			space.Adjust( this, -1 ); // remove this token
		return Task.FromResult( true ); // stopped
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;
}
