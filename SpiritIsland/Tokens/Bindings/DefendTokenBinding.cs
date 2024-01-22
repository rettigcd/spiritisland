namespace SpiritIsland;

public class DefendTokenBinding( SpaceState _tokens ) : IDefendTokenBinding {

	public int Count => _tokens[Token.Defend];

	public void Add( int count ) {
		_tokens.Adjust( Token.Defend, count ); // this should NOT trigger token-added event, Defend are not real tokens.
	}

	public void Clear() => _tokens.Init( Token.Defend, 0 ); // DO NOT Trigger token events, not real token

}