namespace SpiritIsland;

sealed public class SkipExploreTo_Custom : SkipBase, ISkipExploreTo {

	public SkipExploreTo_Custom( string label, bool stopAll, Func<GameCtx, SpaceState, bool> func ) : base( label ) {
		_stopAll = stopAll;
		_func = (a,b) => Task.FromResult(func(a,b));
	}
	public SkipExploreTo_Custom( string label, bool stopAll, Func<GameCtx, SpaceState, Task<bool>> func ) : base( label ) {
		_stopAll = stopAll;
		_func = func;
	}

	public Task<bool> Skip( GameCtx gameState, SpaceState space ) {
		if(!_stopAll)
			space.Adjust( this, -1 );
		return _func( gameState, space );
	}

	readonly Func<GameCtx, SpaceState, Task<bool>> _func;
	readonly bool _stopAll;
}
