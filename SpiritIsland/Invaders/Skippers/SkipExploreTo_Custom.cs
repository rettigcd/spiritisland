namespace SpiritIsland;

sealed public class SkipExploreTo_Custom : BaseModEntity, IEndWhenTimePasses, ISkipExploreTo {

	public SkipExploreTo_Custom( bool stopAll, Func<GameCtx, SpaceState, bool> func ) : base() {
		_stopAll = stopAll;
		_func = (a,b) => Task.FromResult(func(a,b));
	}
	public SkipExploreTo_Custom( bool stopAll, Func<GameCtx, SpaceState, Task<bool>> func ) : base() {
		_stopAll = stopAll;
		_func = func;
	}

	public Task<bool> Skip( GameCtx gameState, SpaceState space ) {
		if(!_stopAll)
			space.Adjust( this, -1 );
		return _func( gameState, space );
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;


	readonly Func<GameCtx, SpaceState, Task<bool>> _func;
	readonly bool _stopAll;
}
