namespace SpiritIsland;

sealed public class SkipExploreTo_Custom : BaseModEntity, IEndWhenTimePasses, ISkipExploreTo {

	public SkipExploreTo_Custom( bool stopAll, Func<SpaceState, bool> func ) : base() {
		_stopAll = stopAll;
		_func = (b) => Task.FromResult(func(b));
	}
	public SkipExploreTo_Custom( bool stopAll, Func<SpaceState, Task<bool>> func ) : base() {
		_stopAll = stopAll;
		_func = func;
	}

	public Task<bool> Skip( SpaceState space ) {
		if(!_stopAll)
			space.Adjust( this, -1 );
		return _func( space );
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;


	readonly Func<SpaceState, Task<bool>> _func;
	readonly bool _stopAll;
}
