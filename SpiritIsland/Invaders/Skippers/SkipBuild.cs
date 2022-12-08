namespace SpiritIsland;

/// <summary> Stops either 1 or ALL builds. </summary>
public class SkipBuild : SkipBase, ISkipBuilds {

	readonly TokenClass[] stoppedClasses;
	readonly UsageDuration duration;

	static public SkipBuild Default( string label ) => new SkipBuild( label, UsageDuration.OneSkipThisTurn,  Invader.Town, Invader.City );
	
	public SkipBuild( string label, UsageDuration duration, params TokenClass[] stoppedTokenClasses ):base(label) {
		this.duration = duration;
		this.stoppedClasses = stoppedTokenClasses;
	}

	bool Stops( TokenClass buildClass ) => stoppedClasses.Contains( buildClass );

	public virtual Task<bool> Skip( GameCtx _, SpaceState space, TokenClass buildClass ) {
		if( !Stops( buildClass ) ) return Task.FromResult(false); // not stopped

		if(duration == UsageDuration.OneSkipThisTurn )
			space.Adjust( this, -1 ); // remove this token

		return Task.FromResult(true); // stopped
	}

}