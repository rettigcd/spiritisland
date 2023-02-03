namespace SpiritIsland;

/// <summary>
/// Base class Token for Skips and mods. Hidden
/// </summary>
/// <remarks>
/// Provides .Class, and ability to auto-cleanup at end of round.
/// </remarks>
public class BaseModToken : ITokenWithEndOfRoundCleanup {

	protected BaseModToken() {
		_keepForever = false;
	}

	protected BaseModToken( bool keepForever ) {
		_keepForever = keepForever;
	}

	public TokenClass Class => ActionModTokenClass.Singleton;

	void ITokenWithEndOfRoundCleanup.EndOfRoundCleanup( SpaceState spaceState ) {
		if( _keepForever ) return; // no cleanup

		if( 1 < spaceState[this] )
			throw new Exception("We shouldn't have 2 tokens of this.");
		spaceState.Init(this,0);
	}

	readonly bool _keepForever;

}
