namespace SpiritIsland;

sealed public class SkipBuild_Custom( string label, bool stopAll, Func<SpaceState, bool> func ) 
	: BaseModEntity(), IEndWhenTimePasses, ISkipBuilds 
{

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public string Text { get; } = label;

	public Task<bool> Skip( SpaceState space ) {
		if( !stopAll )
			space.Adjust( this, -1 );
		return _func( space );
	}
	readonly Func<SpaceState, Task<bool>> _func = ( ss ) => Task.FromResult( func( ss ) );
}
