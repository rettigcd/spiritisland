namespace SpiritIsland;

static public class SpaceStateExtensions {

	public static IEnumerable<string> SelectLabels( this IEnumerable<SpaceState> spaceStates ) => spaceStates.Select( x => x.Space.Text );

	public static IEnumerable<Space> Downgrade( this IEnumerable<SpaceState> spaceStates ) => spaceStates.Select( x => x.Space );

	public static IEnumerable<Space> IsInPlay( this IEnumerable<Space> spaces ) => spaces.Where( TerrainMapper.Current.IsInPlay );

	public static IEnumerable<SpaceState> ScopeTokens(this IEnumerable<Space> spaces) => spaces.Select(ActionScope.Current.AccessTokens);

	public static IEnumerable<SpaceToken> WhereIsOn( this IEnumerable<SpaceToken> spaceTokens, IEnumerable<SpaceState> spaceStates ) {
		var validSpaces = spaceStates.Select( x => x.Space ).ToHashSet();
		return spaceTokens.Where( t => validSpaces.Contains( t.Space ) );
	}

	public static IEnumerable<SpaceState> IsInPlay( this IEnumerable<SpaceState> spaces )
		=> spaces.Where( x => TerrainMapper.Current.IsInPlay( x.Space ) );

	public static IEnumerable<Space> ISInPlay( this IEnumerable<Space> spaces )
		=> spaces.Where( TerrainMapper.Current.IsInPlay );

	/// <summary> Convenience method for fluid syntax. </summary>
	static public Task DamageLand( this SpaceState tokens, int totalLandDamage ) 
		=> LandDamage.Add(tokens,totalLandDamage);

	static public void AddFear( this SpaceState tokens, int count, FearType fearType = FearType.Direct )
		=> GameState.Current.Fear.AddOnSpace( tokens, count, fearType );


	#region SetUp / Adjust

	/// <summary>
	/// Separate method used during game initialization, not requiring Token Events/animations
	/// </summary>
	static public void Setup( this SpaceState tokens, IToken token, int delta ) 
		=> tokens.Adjust( token, delta );

	static public void Setup( this SpaceState tokens, HumanTokenClass tokenClass, int delta ) 
		=> tokens.Adjust( tokens.GetDefault( tokenClass ), delta );

	#endregion SetUp / Adjust

	#region Invader

	/// <summary> Gets the invader we most want to be rid of. </summary>
	static public HumanToken BestInvaderToBeRidOf( this SpaceState ss, ITag[] tags ) {
		return ss.OfAnyTag( tags )
			.Cast<HumanToken>()
			.OrderByDescending( g => g.FullHealth )
			.ThenBy( k => k.StrifeCount )  // un-strifed first
			.ThenByDescending( g => g.RemainingHealth )
			.FirstOrDefault();
	}

	#endregion

}