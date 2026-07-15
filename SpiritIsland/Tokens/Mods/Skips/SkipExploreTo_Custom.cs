namespace SpiritIsland;

public abstract class SkipExploreTo_Custom(bool stopAll) : BaseModEntity, IEndWhenTimePasses, ISkipExploreTo {

	public Task<bool> Skip(Space space) {
		if( !stopAll )
			space.Adjust(this, -1);
		return Task.FromResult(ShouldSkip(space));
	}

	protected abstract bool ShouldSkip(Space space);

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;
}
