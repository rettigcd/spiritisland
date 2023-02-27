namespace SpiritIsland;

sealed public class SkipBuild_Custom : BaseModEntity, IEndWhenTimePasses, ISkipBuilds {

	public SkipBuild_Custom( string label, bool stopAll, Func<SpaceState, bool> func ) : base() {
		Text = label;
		_stopAll = stopAll;
		_func = ( ss ) => Task.FromResult( func( ss ) );
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public string Text { get; }

	public Task<bool> Skip( SpaceState space ) {
		if( !_stopAll )
			space.Adjust( this, -1 );
		return _func( space );
	}
	readonly Func<SpaceState, Task<bool>> _func;
	readonly bool _stopAll;
}
