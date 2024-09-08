namespace SpiritIsland;

/// <summary> Stops 1 Ravage. </summary>
public class SkipRavage(string label, UsageDuration duration = UsageDuration.SkipOneThisTurn)
	: BaseModEntity(), IEndWhenTimePasses, ISkipRavages {

	/// <summary> So we can log what it was that stopped the ravage. </summary>
	public string SourceLabel { get; } = label;

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public virtual Task<bool> Skip(Space space) {
		if( _duration == UsageDuration.SkipOneThisTurn )
			space.Adjust(this, -1); // remove this token
		return Task.FromResult(true); // stopped
	}

	#region private

	readonly UsageDuration _duration = duration;

	#endregion

}

