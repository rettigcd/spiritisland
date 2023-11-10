namespace SpiritIsland.A;
using SI_SpaceToken = SpiritIsland.SpaceToken;
using SI_Space = SpiritIsland.Space;
using IEnumerable_SpaceToken = IEnumerable<SpiritIsland.SpaceToken>;

// For Selecting Token from multiple spaces
public class SpaceToken : TypedDecision<SI_SpaceToken>, IHaveArrows {

	/// <summary> Adds Adjacent Info for Collecting (moving/gathering) </summary>
	static public SpaceToken ToCollect( string prompt, IEnumerable_SpaceToken tokens, Present present, SI_Space to )
		=> new SpaceToken( prompt, tokens, present ).PointArrowTo( to );

	public static SpaceToken ToPush( int count, IEnumerable_SpaceToken options, Present present )
		=> new SpaceToken( FormatLabel( "Push", count, present ), options, present );

	public static SpaceToken ToMove( int count, IEnumerable_SpaceToken options, Present present )
		=> new SpaceToken( FormatLabel( "Move", count, present ), options, present );

	public static SpaceToken ToRemove( int count, IEnumerable_SpaceToken options, Present present )
		=> new SpaceToken( FormatLabel( "Remove", count, present ), options, present );

	static string FormatLabel( string action, int count, Present present ) 
		=> count != int.MaxValue 
		? (present != Present.Done 
			? $"{action} ({count})" 
			: $"{action} up to ({count})")
		: (present != Present.Done ? $"{action} all" : $"{action}");

	public static SpaceToken OfDeployedPresence( string prompt, SpiritIsland.Spirit spirit, Present present = Present.Always )
		=> new SpiritIsland.A.SpaceToken( prompt, spirit.Presence.Deployed, present );

	#region constructor

	/// <summary>  Constructs SpaceToken options for MANY spaces  </summary>
	public SpaceToken( string prompt, IEnumerable<SI_SpaceToken> tokens, Present present )
		: base( prompt, tokens, present ) {
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
