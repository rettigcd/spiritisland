namespace SpiritIsland;

/// <summary> Stops 1 Ravage. </summary>
public class SkipRavage : ActionModBaseToken, ISkipRavages {

	public SkipRavage( string label ) : base( label ) { }

	public virtual Task<bool> Skip( GameState _, SpaceState space ) {
		space.Adjust( this, -1 ); // remove this token
		return Task.FromResult( true ); // stopped
	}

}

