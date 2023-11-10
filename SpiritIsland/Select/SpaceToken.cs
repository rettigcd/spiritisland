namespace SpiritIsland.A;
using Orig_SpaceToken = SpiritIsland.SpaceToken;
using Orig_Space = SpiritIsland.Space;

// For Selecting Token from multiple spaces
public class SpaceToken : TypedDecision<Orig_SpaceToken>, IHaveArrows {

	/// <summary> Adds Adjacent Info for Collecting (moving/gathering) </summary>
	static public SpaceToken ToCollect( string prompt, IEnumerable<Orig_SpaceToken> tokens, Present present, Orig_Space to ) 
		=> new SpaceToken( prompt, tokens, present ).PointArrowTo( to );

	public static SpaceToken ToPush( Orig_Space space, int count, IToken[] options, Present present )
		=> new SpaceToken( present != Present.Done ? $"Push ({count})" : $"Push up to ({count})", space, options, present );

	public static SpaceToken ToMove( Orig_Space srcSpace, int count, IToken[] options, Present present )
		=> new SpaceToken( present != Present.Done ? $"Move ({count})" : $"Move up to ({count})", srcSpace, options, present );

	public static SpaceToken ToRemove( Orig_Space space, int count, IToken[] options, Present present )
		=> new SpaceToken( present != Present.Done ? $"Remove ({count})" : $"Remove up to ({count})", space, options, present );

	#region constructor

	/// <summary> 
	/// Constructs SpaceToken options for 1 space
	/// NO arrows
	/// </summary>
	public SpaceToken( string prompt, Orig_Space sourceSpace, IEnumerable<IToken> options, Present present )
		: base( prompt, options.Select( t => new Orig_SpaceToken( sourceSpace, t, false ) ), present ) 
	{
		SpaceTokens = options.Select( t => new Orig_SpaceToken( sourceSpace, t, false ) ).ToArray();
	}

	/// <summary> 
	/// Constructs SpaceToken options for MANY spaces 
	/// </summary>
	public SpaceToken( string prompt, IEnumerable<Orig_SpaceToken> tokens, Present present )
		: base( prompt, tokens, present ) 
	{
		SpaceTokens = tokens.ToArray();
	}

	#endregion

	public Orig_SpaceToken[] SpaceTokens { get; }

	public SpaceToken PointArrowTo( Orig_Space destination ) {
		Destination = destination;
		return this;
	}

	public Orig_Space Destination { get; private set; }

	public IEnumerable<Arrow> Arrows => Destination == null
		? Enumerable.Empty<Arrow>()
		: SpaceTokens.Select( st => new Arrow { Token = st.Token, From = st.Space, To = Destination } );


}