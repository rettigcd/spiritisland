namespace SpiritIsland;

/// <summary> Stops either 1 or ALL builds. </summary>
public class SkipBuild : BaseModEntity, IEndWhenTimePasses, ISkipBuilds {

	readonly ITokenClass[] _stoppedClasses;
	readonly UsageDuration _duration;

	static public SkipBuild Default( string label ) => new SkipBuild( label, UsageDuration.SkipOneThisTurn );
	
	public SkipBuild( string label, UsageDuration duration, params ITokenClass[] stoppedTokenClasses ):base() {
		Text = label;
		_duration = duration;

		if(stoppedTokenClasses.Length == 0) stoppedTokenClasses = null; // replace
		_stoppedClasses = stoppedTokenClasses ?? Human.Town_City;
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public string Text { get; }

	bool Stops( ITokenClass buildClass ) => _stoppedClasses.Contains( buildClass );

	public virtual Task<bool> Skip( Space space ) {
		if( !Stops( BuildEngine.InvaderToAdd.Value ) ) return Task.FromResult(false); // not stopped

		if(_duration == UsageDuration.SkipOneThisTurn )
			space.Adjust( this, -1 ); // remove this token

		return Task.FromResult(true); // stopped
	}

}