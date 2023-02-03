namespace SpiritIsland;

/// <summary> Stops either 1 or ALL builds. </summary>
public class SkipBuild : BaseModToken, ISkipBuilds {

	readonly TokenClass[] _stoppedClasses;
	readonly UsageDuration _duration;

	static public SkipBuild Default( string label ) => new SkipBuild( label, UsageDuration.OneSkipThisTurn,  Human.Town_City );
	
	public SkipBuild( string label, UsageDuration duration, params TokenClass[] stoppedTokenClasses ):base() {
		Text = label;
		_duration = duration;
		_stoppedClasses = stoppedTokenClasses;
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public string Text { get; }

	bool Stops( TokenClass buildClass ) => _stoppedClasses.Contains( buildClass );

	public virtual Task<bool> Skip( GameCtx _, SpaceState space, TokenClass buildClass ) {
		if( !Stops( buildClass ) ) return Task.FromResult(false); // not stopped

		if(_duration == UsageDuration.OneSkipThisTurn )
			space.Adjust( this, -1 ); // remove this token

		return Task.FromResult(true); // stopped
	}

}