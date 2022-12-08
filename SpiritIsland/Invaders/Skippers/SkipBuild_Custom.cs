namespace SpiritIsland;

sealed public class SkipBuild_Custom : SkipBase, ISkipBuilds {

	public SkipBuild_Custom( string label, bool stopAll, Func<GameCtx, SpaceState, TokenClass, bool> func ) : base( label ) {
		_stopAll = stopAll;
		_func = (a,b,c) => Task.FromResult(func(a,b,c));
	}

	public SkipBuild_Custom( string label, bool stopAll, Func<GameCtx, SpaceState, TokenClass, Task<bool>> func ) : base( label ) {
		_stopAll = stopAll;
		_func = func;
	}

	public Task<bool> Skip( GameCtx gameState, SpaceState space, TokenClass buildClass ) {
		if( !_stopAll )
			space.Adjust( this, -1 );
		return _func( gameState, space, buildClass );
	}
	readonly Func<GameCtx, SpaceState, TokenClass, Task<bool>> _func;
	readonly bool _stopAll;
}
