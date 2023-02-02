namespace SpiritIsland;

/// <summary>
/// Base class Token for Skips and mods. Hidden
/// </summary>
public class BaseModToken : ITokenWithEndOfRoundCleanup {

	protected BaseModToken( string label, UsageCost cost, bool keepForever = false ) {
		Text = label;
		Cost = cost;
		_keepForever = keepForever;
	}

	public UsageCost Cost { get; } // not needed by everything, but needed by most

	public string Text { get; }

	public TokenClass Class => ActionModTokenClass.Singleton;

	void ITokenWithEndOfRoundCleanup.EndOfRoundCleanup( SpaceState spaceState ) {
		if( _keepForever ) return; // no cleanup

		if( 1 < spaceState[this] )
			throw new Exception("We shouldn't have 2 tokens of this.");
		spaceState.Init(this,0);
	}

	readonly bool _keepForever;

}

class ActionModTokenClass : TokenClass {

	public static readonly TokenClass Singleton = new ActionModTokenClass();

	static readonly public TokenClass Class = new ActionModTokenClass();
	public string Label => "Mod";

	public TokenCategory Category => TokenCategory.Skipper;
}
