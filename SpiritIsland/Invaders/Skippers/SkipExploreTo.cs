namespace SpiritIsland;

/// <summary> Stops either 1 or ALL explores </summary>
public class SkipExploreTo : BaseModToken, ISkipExploreTo {

	public SkipExploreTo( string label ):base(label, UsageCost.Free ) { }

	public virtual Task<bool> Skip( GameCtx _, SpaceState space ) {
		space.Adjust( this, -1 ); // remove this token
		return Task.FromResult( true ); // stopped
	}

}
