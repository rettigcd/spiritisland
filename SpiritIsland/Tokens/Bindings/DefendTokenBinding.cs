namespace SpiritIsland;

public class DefendTokenBinding : IDefendTokenBinding {

	public DefendTokenBinding( SpaceState tokens ) {
		this.tokens = tokens;
	}

	public int Count => tokens[Token.Defend];

	public void Add( int count ) {
		tokens.Adjust( Token.Defend, count ); // this should NOT trigger token-added event, Defend are not real tokens.
	}

	public void Clear() => tokens.Init( Token.Defend, 0 ); // DO NOT Trigger token events, not real token

	readonly SpaceState tokens;

}