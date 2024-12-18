namespace SpiritIsland;

static public class SpaceExtensions {

	public static IEnumerable<string> SelectLabels( this IEnumerable<Space> spaces ) => spaces.Select( x => x.SpaceSpec.Label );

	public static IEnumerable<Space> ScopeTokens(this IEnumerable<SpaceSpec> spaces) => spaces.Select(ActionScope.Current.AccessTokens);

	public static IEnumerable<SpaceToken> WhereIsOn( this IEnumerable<SpaceToken> spaceTokens, IEnumerable<Space> spaces ) {
		var validSpaces = spaces.Select( x => x.SpaceSpec ).ToHashSet();
		return spaceTokens.Where( t => validSpaces.Contains( t.Space.SpaceSpec ) );
	}

	public static IEnumerable<Space> IsInPlay( this IEnumerable<Space> spaces )
		=> spaces.Where( TerrainMapper.Current.IsInPlay );

	/// <summary> Convenience method for fluid syntax. </summary>
	static public Task DamageLand( this Space space, int totalLandDamage ) 
		=> LandDamage.Add(space,totalLandDamage);

	static public Task AddFear(this Space space, int count, FearType fearType = FearType.Direct)
		=> GameState.Current.Fear.AddOnSpace(space, count, fearType);

	static public void TransferAllTokensTo( this Space from,  Space to, bool copyInvisible ) {
		foreach( var key in from.Keys.ToArray() ) {
			int count = from[key];
			if( key is IToken ) {
				// move visible
				from.Adjust(key, -count);
				to.Adjust(key, count);
			} else if( copyInvisible )
				// copy invisible (orig keep their invisible mods)
				to.Adjust(key, count);
		}
	}


	#region SetUp / Adjust

	/// <summary>
	/// Separate method used during game initialization, not requiring Token Events/animations
	/// </summary>
	static public void Setup( this Space space, IToken token, int delta ) 
		=> space.Adjust( token, delta );

	static public void Setup( this Space space, HumanTokenClass tokenClass, int delta ) 
		=> space.Adjust( space.GetDefault( tokenClass ), delta );

	#endregion SetUp / Adjust

	#region Invader

	/// <summary> Gets the invader we most want to be rid of. </summary>
	static public HumanToken? BestInvaderToBeRidOf( this Space ss, ITag[] tags ) {
		return ss.OfAnyTag( tags )
			.Cast<HumanToken>()
			.OrderByDescending( g => g.FullHealth )
			.ThenBy( k => k.StrifeCount )  // un-strifed first
			.ThenByDescending( g => g.RemainingHealth )
			.FirstOrDefault();
	}

	#endregion

}