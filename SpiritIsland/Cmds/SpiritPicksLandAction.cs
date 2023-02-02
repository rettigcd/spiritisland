namespace SpiritIsland;

/// <summary>
/// Spirit/Player can pick from any land.
/// </summary>
public class SpiritPicksLandAction : IExecuteOn<SelfCtx> {

	public SpiritPicksLandAction( IExecuteOn<TargetSpaceCtx> spaceAction, string andInToForPreposition ) { 
		_spaceAction = spaceAction;
		_landPreposition = andInToForPreposition;
	}

	public string Description => $"{_spaceAction.Description} {_landPreposition} {_diffString}{LandCriteria.Description}";

	public bool IsApplicable( SelfCtx ctx ) => true;

	public async Task Execute( SelfCtx ctx ) {
		var spaceOptions = ctx.GameState.AllActiveSpaces
			.Where( x => !_disallowedSpaces.Contains( x.Space ) ) // for picking Different spaces
			.Select( s => ctx.Target( s.Space ) )
			.Where( ctx => ctx.IsInPlay )       // land-only, exclude normal ocean
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

		await _spaceAction.Execute( spaceCtx );
	}

	#region Configue Methods

	public SpiritPicksLandAction Which( TargetSpaceCtxFilter filter ) { _landCriteria = filter; return this; }
	public SpiritPicksLandAction MakeOptional() { _present = Present.Done; return this; }
	public SpiritPicksLandAction AllDifferent() {  _chooseDifferentLands = true; return this; }
	public SpiritPicksLandAction ByPickingToken( params TokenClass[] tokenClasses ) { _firstPickTokenClasses = tokenClasses; return this; }

	#endregion

	#region private

	async Task<TargetSpaceCtx> PickSpaceBySelectingToken( SelfCtx ctx, TargetSpaceCtx[] spaceOptions ) {
		// Get options
		IEnumerable<SpaceToken> GetSpaceTokens( TargetSpaceCtx x ) => x.Tokens.OfAnyClass( _firstPickTokenClasses ).Cast<IVisibleToken>().Select( t => new SpaceToken( x.Space, t ) );
		SpaceToken[] spaceTokenOptions = spaceOptions.SelectMany( GetSpaceTokens ).ToArray();

		// Select
		SpaceToken st = await ctx.Self.Gateway.Decision( new Select.TokenFromManySpaces( "Select token for " + _spaceAction.Description, spaceTokenOptions, Present.Always ) );

		if(st != null) {
			ctx.Self.Gateway.Preloaded = st;
			return ctx.Target( st.Space );
		}

		// !!! Bug - We need to Load into the Preloaded property a 'no-choice/null' option so the auto-picker knows to pick nothing.
		return null;

	}
	string _diffString => _chooseDifferentLands ? "different " : "";
	TargetSpaceCtxFilter _landCriteria;
	readonly IExecuteOn<TargetSpaceCtx> _spaceAction;
	readonly HashSet<Space> _disallowedSpaces = new HashSet<Space>();

	// configurable
	TargetSpaceCtxFilter LandCriteria => _landCriteria ??= Is.AnyLand;

	readonly string _landPreposition;
	Present _present = Present.Always;
	bool _chooseDifferentLands = false;
	TokenClass[] _firstPickTokenClasses;

	#endregion
}
