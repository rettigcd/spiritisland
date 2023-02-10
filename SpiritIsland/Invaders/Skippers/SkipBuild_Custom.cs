namespace SpiritIsland;

sealed public class SkipBuild_Custom : SelfCleaningToken, ISkipBuilds {

	public SkipBuild_Custom( string label, bool stopAll, Func<SpaceState, bool> func ) : base() {
		Text = label;
		_stopAll = stopAll;
		_func = ( ss, _ ) => Task.FromResult( func( ss ) );
	}

	//public SkipBuild_Custom( string label, bool stopAll, Func<SpaceState, TokenClass, bool> func ) : base() {
	//	Text = label;
	//	_stopAll = stopAll;
	//	_func = ( ss, buidingToken ) => Task.FromResult( func( ss, buidingToken ) );
	//}

	//public SkipBuild_Custom( string label, bool stopAll, Func<SpaceState, TokenClass, Task<bool>> func ) : base() {
	//	Text = label;
	//	_stopAll = stopAll;
	//	_func = func;
	//}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public string Text { get; }

	public Task<bool> Skip( SpaceState space, IEntityClass buildClass ) {
		if( !_stopAll )
			space.Adjust( this, -1 );
		return _func( space, buildClass );
	}
	readonly Func<SpaceState, IEntityClass, Task<bool>> _func;
	readonly bool _stopAll;
}
