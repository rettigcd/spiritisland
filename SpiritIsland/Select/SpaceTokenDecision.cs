namespace SpiritIsland.A;

// For Selecting Token from multiple spaces
public class SpaceTokenDecision : TypedDecision<SpaceToken>, IHaveArrows {

	/// <summary> Adds Adjacent Info for Collecting (moving/gathering) </summary>
	static public SpaceTokenDecision ToCollect( string prompt, IEnumerable<SpaceToken> tokens, Present present, SpaceSpec to )
		=> new SpaceTokenDecision( prompt, tokens, present ).PointArrowTo( to );

	public static SpaceTokenDecision OfDeployedPresence( string prompt, SpiritIsland.Spirit spirit, Present present = Present.Always )
		=> new SpiritIsland.A.SpaceTokenDecision( prompt, spirit.Presence.Deployed, present );

	#region constructor

	/// <summary>  Constructs SpaceToken options for MANY spaces  </summary>
	public SpaceTokenDecision( string prompt, IEnumerable<SpaceToken> tokens, Present present )
		: base( prompt, tokens.ToArray() /* get rid of generators, make concrete so we can mod .ShowSpace property */, present )
	{
		SpaceTokens = _allOptions.OfType<SpaceToken>().ToArray();
	}

	#endregion

	public SpaceToken[] SpaceTokens { get; }

	public SpaceTokenDecision PointArrowTo( SpaceSpec destination ) {
		Destination = destination;
		return this;
	}

	public SpaceSpec Destination { get; private set; }

	public IEnumerable<Arrow> Arrows => Destination == null
		? Enumerable.Empty<Arrow>()
		: SpaceTokens.Select( st => new Arrow { Token = st.Token, From = st.Space.SpaceSpec, To = Destination } );

}

public class MyTokenOn( string prompt, IEnumerable<TokenLocation> options, Present presenct ) 
	: TypedDecision<TokenLocation>( prompt, options, presenct )
{
	public TokenLocation[] TokensOn { get; } = options.ToArray();
}