namespace SpiritIsland.A;
using SI_SpaceToken = SpiritIsland.SpaceToken;
using SI_Space = SpiritIsland.Space;

// For Selecting Token from multiple spaces
public class SpaceToken : TypedDecision<SI_SpaceToken>, IHaveArrows {

	/// <summary> Adds Adjacent Info for Collecting (moving/gathering) </summary>
	static public SpaceToken ToCollect( string prompt, IEnumerable<SI_SpaceToken> tokens, Present present, SI_Space to ) 
		=> new SpaceToken( prompt, tokens, present ).PointArrowTo( to );

	// !!!!!!!!!!!!!!!
	public static SpaceToken ToPush( SI_Space space, int count, IToken[] options, Present present )
		=> new SpaceToken( present != Present.Done ? $"Push ({count})" : $"Push up to ({count})", space, options, present );

	// !!!!!!!!!!!!!!!
	public static SpaceToken ToMove( SI_Space srcSpace, int count, IToken[] options, Present present )
		=> new SpaceToken( present != Present.Done ? $"Move ({count})" : $"Move up to ({count})", srcSpace, options, present );

	// !!!!!!!!!!!!!!!
	public static SpaceToken ToRemove( SI_Space space, int count, IToken[] options, Present present )
		=> new SpaceToken( present != Present.Done ? $"Remove ({count})" : $"Remove up to ({count})", space, options, present );

	#region constructor

	/// <summary>  Constructs SpaceToken options for 1 space, NO arrows </summary>
	public SpaceToken( string prompt, SI_Space sourceSpace, IEnumerable<IToken> options, Present present )
		: base( prompt, options.Select( t => new SI_SpaceToken( sourceSpace, t, false ) ), present ) 
	{
		SpaceTokens = options.Select( t => new SI_SpaceToken( sourceSpace, t, false ) ).ToArray();
	}

	/// <summary>  Constructs SpaceToken options for MANY spaces  </summary>
	public SpaceToken( string prompt, IEnumerable<SI_SpaceToken> tokens, Present present )
		: base( prompt, tokens, present ) 
	{
		SpaceTokens = tokens.ToArray();
	}

	#endregion

	public SI_SpaceToken[] SpaceTokens { get; }

	public SpaceToken PointArrowTo( SI_Space destination ) {
		Destination = destination;
		return this;
	}

	public SI_Space Destination { get; private set; }

	public IEnumerable<Arrow> Arrows => Destination == null
		? Enumerable.Empty<Arrow>()
		: SpaceTokens.Select( st => new Arrow { Token = st.Token, From = st.Space, To = Destination } );


}