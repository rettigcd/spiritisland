namespace SpiritIsland.Select;

// For Selecting Token from multiple spaces
public class TokenFromManySpaces : TypedDecision<SpaceToken>, IHaveArrows {

	/// <summary> Adds Adjacent Info for Collecting (moving/gathering) </summary>
	static public TokenFromManySpaces ToCollect( string prompt, IEnumerable<SpaceToken> tokens, Present present, SpiritIsland.Space to ) 
		=> new TokenFromManySpaces( prompt, tokens, present, to );

	#region constructor

	/// <summary> 
	/// Constructs SpaceToken options for 1 space
	/// NO arrows
	/// </summary>
	public TokenFromManySpaces( string prompt, SpiritIsland.Space sourceSpace, IEnumerable<IToken> options, Present present )
		: base( prompt, options.Select( t => new SpaceToken( sourceSpace, t, false ) ), present ) 
	{
		SpaceTokens = options.Select( t => new SpaceToken( sourceSpace, t, false ) ).ToArray();
	}

	/// <summary> 
	/// Constructs SpaceToken options for MANY spaces 
	/// NO arrows
	/// </summary>
	public TokenFromManySpaces( string prompt, IEnumerable<SpaceToken>tokens, Present present )
		: base( prompt, tokens, present ) 
	{
		SpaceTokens = tokens.ToArray();
	}

	/// <summary> 
	/// Constructs SpaceToken options for MANY spaces 
	/// WITH arrows
	/// </summary>
	public TokenFromManySpaces( string prompt, IEnumerable<SpaceToken> tokens, Present present, SpiritIsland.Space destination )
		: base( prompt, tokens, present ) 
	{
		SpaceTokens = tokens.ToArray();
		Destination = destination;
	}

	#endregion

	public SpaceToken[] SpaceTokens { get; }

	public SpiritIsland.Space Destination { get; private set; }

	public IEnumerable<Arrow> Arrows => Destination == null
		? Array.Empty<Arrow>()
		: SpaceTokens.Select( st => new Arrow { Token = st.Token, From = st.Space, To = Destination } );


}