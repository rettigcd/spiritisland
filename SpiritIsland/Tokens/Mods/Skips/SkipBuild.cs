namespace SpiritIsland;

/// <summary> Stops either 1 or ALL builds. </summary>
public class SkipBuild(string label, UsageDuration duration, params ITokenClass[] stoppedTokenClasses) : BaseModEntity(), IEndWhenTimePasses, ISkipBuilds {

	readonly ITokenClass[] _stoppedClasses = stoppedTokenClasses.Length > 0 ? stoppedTokenClasses : Human.Town_City;

	static public SkipBuild Default(string label) => new SkipBuild(label, UsageDuration.SkipOneThisTurn);

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public string Text { get; } = label;

	bool Stops(ITokenClass buildClass) => _stoppedClasses.Contains(buildClass);

	public virtual Task<bool> Skip(Space space) {
		if( !Stops(BuildEngine.InvaderToAdd.Value!) ) return Task.FromResult(false); // not stopped

		if( duration == UsageDuration.SkipOneThisTurn )
			space.Adjust(this, -1); // remove this token

		return Task.FromResult(true); // stopped
	}

}