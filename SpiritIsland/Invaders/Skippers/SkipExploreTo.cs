namespace SpiritIsland;

/// <summary> Stops either 1 or ALL explores </summary>
public class SkipExploreTo : BaseModEntity, IEndWhenTimePasses, ISkipExploreTo {

	public SkipExploreTo(bool skipAll=false):base() {
		_skipAll = skipAll;
	}

	public virtual Task<bool> Skip( SpaceState space ) {
		if(!_skipAll)
			space.Adjust( this, -1 ); // remove this token
		return Task.FromResult( true ); // stopped
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	readonly bool _skipAll;

}
