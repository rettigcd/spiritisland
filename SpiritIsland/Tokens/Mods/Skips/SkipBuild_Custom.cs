namespace SpiritIsland;

public abstract class SkipBuild_Custom(string label, bool stopAll)
	: BaseModEntity, IEndWhenTimePasses, ISkipBuilds {

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public string Text { get; } = label;

	public Task<bool> Skip(Space space) {
		if( !stopAll )
			space.Adjust(this, -1);
		return Task.FromResult(ShouldSkip(space));
	}

	protected abstract bool ShouldSkip(Space space);
}
