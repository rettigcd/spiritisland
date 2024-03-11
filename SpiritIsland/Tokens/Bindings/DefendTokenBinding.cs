namespace SpiritIsland;

public class DefendTokenBinding( Space _space ) : IDefendTokenBinding {

	public int Count => _space[Token.Defend];

	public void Add( int count ) {
		_space.Adjust( Token.Defend, count ); // this should NOT trigger token-added event, Defend are not real tokens.
	}

	public void Clear() => _space.Init( Token.Defend, 0 ); // DO NOT Trigger token events, not real token

}