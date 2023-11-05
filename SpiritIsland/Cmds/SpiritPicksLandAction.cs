namespace SpiritIsland;

/// <summary>
/// Spirit/Player can pick from any land.
/// </summary>
public class SpiritPicksLandAction : IActOn<SelfCtx> {

	public SpiritPicksLandAction( IActOn<TargetSpaceCtx> spaceAction, string andInToForPreposition ) { 
		_spaceAction = spaceAction;
		_landPreposition = andInToForPreposition;
	}

	public string Description => $"{_spaceAction.Description} {_landPreposition} {_diffString}{LandCriteria.Description}";

	public bool IsApplicable( SelfCtx ctx ) => true;

	public async Task ActAsync( SelfCtx ctx ) {

		for(int i = 0; i < _landsPerSpirit; ++i) {

			var spaceOptions = GameState.Current.Spaces
				.Where( x => !_disallowedSpaces.Contains( x.Space ) ) // for picking Different spaces
				.Select( s => ctx.Target( s.Space ) )
				.Where( _spaceAction.IsApplicable )  // Matches action Criteria
				.Where( LandCriteria.Filter )
				.ToArray();
			if(spaceOptions.Length == 0) return;

			TargetSpaceCtx spaceCtx = _firstPickTokenClasses != null
				? await PickSpaceBySelectingToken( ctx, spaceOptions )
				: await ctx.SelectSpace( "Select space to " + _spaceAction.Description, spaceOptions.Select( x => x.Space ), _present );

			if(spaceCtx == null) return;

			if(_chooseDifferentLands)
				_disallowedSpaces.Add( spaceCtx.Space );

			await _spaceAction.ActAsync( spaceCtx );
		}
	}

	#region Configue Methods

	public SpiritPicksLandAction Which( TargetSpaceCtxFilter filter ) { _landCriteria = filter; return this; }
	public SpiritPicksLandAction MakeOptional() { _present = Present.Done; return this; }
	public SpiritPicksLandAction AllDifferent() {  _chooseDifferentLands = true; return this; }
	public SpiritPicksLandAction ByPickingToken( params IEntityClass[] tokenClasses ) { _firstPickTokenClasses = tokenClasses; return this; }
	public SpiritPicksLandAction EachSpiritPicks( int count ) { _landsPerSpirit = count; return this; }

	#endregion

	#region private

	async Task<TargetSpaceCtx> PickSpaceBySelectingToken( SelfCtx ctx, TargetSpaceCtx[] spaceOptions ) {
		// Get options
		IEnumerable<SpaceToken> GetSpaceTokens( TargetSpaceCtx x ) => x.Tokens.SpaceTokensOfAnyClass( _firstPickTokenClasses );
		SpaceToken[] spaceTokenOptions = spaceOptions.SelectMany( GetSpaceTokens ).ToArray();

		// Select
		SpaceToken st = await ctx.Self.Gateway.Decision( new Select.ASpaceToken( "Select token for " + _spaceAction.Description, spaceTokenOptions, Present.Always ) );
		ctx.Self.Gateway.Preloaded = st;

		return st == null ? null : ctx.Target( st.Space );
	}
	string _diffString => _chooseDifferentLands ? "different " : "";
	TargetSpaceCtxFilter _landCriteria;
	readonly IActOn<TargetSpaceCtx> _spaceAction;
	readonly HashSet<Space> _disallowedSpaces = new HashSet<Space>();

	// configurable
	TargetSpaceCtxFilter LandCriteria => _landCriteria ??= Is.AnyLand;

	readonly string _landPreposition;
	Present _present = Present.Always;
	bool _chooseDifferentLands = false;
	IEntityClass[] _firstPickTokenClasses;
	int _landsPerSpirit = 1;
	#endregion
}
