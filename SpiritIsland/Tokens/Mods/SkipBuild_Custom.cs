namespace SpiritIsland;

sealed public class SkipBuild_Custom(string label, bool stopAll, Func<Space, bool> func)
	: BaseModEntity(), IEndWhenTimePasses, ISkipBuilds {

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public string Text { get; } = label;

	public Task<bool> Skip(Space space) {
		if( !stopAll )
			space.Adjust(this, -1);
		return _func(space);
	}
	readonly Func<Space, Task<bool>> _func = (ss) => Task.FromResult(func(ss));
}
