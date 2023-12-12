namespace SpiritIsland.A;
using SI_SpaceToken = SpiritIsland.SpaceToken;
using SI_Space = SpiritIsland.Space;
using IEnumerable_SpaceToken = IEnumerable<SpiritIsland.SpaceToken>;

// For Selecting Token from multiple spaces
public class SpaceToken : TypedDecision<SI_SpaceToken>, IHaveArrows {

	/// <summary> Adds Adjacent Info for Collecting (moving/gathering) </summary>
	static public SpaceToken ToCollect( string prompt, IEnumerable_SpaceToken tokens, Present present, SI_Space to )
		=> new SpaceToken( prompt, tokens, present ).PointArrowTo( to );

	public static SpaceToken OfDeployedPresence( string prompt, SpiritIsland.Spirit spirit, Present present = Present.Always )
		=> new SpiritIsland.A.SpaceToken( prompt, spirit.Presence.Deployed, present );

	#region constructor

	/// <summary>  Constructs SpaceToken options for MANY spaces  </summary>
	public SpaceToken( string prompt, IEnumerable<SI_SpaceToken> tokens, Present present )
		: base( prompt, tokens.ToArray() /* get rid of generators, make concrete so we can mod .ShowSpace property */, present )
	{
		SpaceTokens = _allOptions.OfType<SI_SpaceToken>().ToArray();
		int count = SpaceTokens.Select( st => st.Space ).Distinct().Count();
		var configText = 1 < count 
			? SI_SpaceToken.ConfigSpaceTokenText.Both 
			: SI_SpaceToken.ConfigSpaceTokenText.Token;
		foreach(SI_SpaceToken st in SpaceTokens) 
			st.ConfigText = configText;
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
