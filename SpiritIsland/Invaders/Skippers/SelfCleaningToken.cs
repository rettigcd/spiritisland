namespace SpiritIsland;

/// <summary>
/// Cleans up itself at end of round.
/// Base class Token for Skips and mods.
/// </summary>
/// <remarks>
/// Provides .Class, and ability to auto-cleanup at end of round.
/// </remarks>
public class SelfCleaningToken : ITokenWithEndOfRoundCleanup {

	/// <summary> Automatically removes token at end of round. </summary>
	protected SelfCleaningToken() {
		_keepForever = false;
	}

	protected SelfCleaningToken( bool keepForever ) {
		_keepForever = keepForever;
	}

	public IEntityClass Class => ActionModTokenClass.Singleton;

	void ITokenWithEndOfRoundCleanup.EndOfRoundCleanup( SpaceState spaceState ) {
		if( _keepForever ) return; // no cleanup

		if( 1 < spaceState[this] )
			throw new Exception("We shouldn't have 2 tokens of this.");
		spaceState.Init(this,0);
	}

	readonly bool _keepForever;

}
