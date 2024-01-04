namespace SpiritIsland;

/// <summary>
/// Spirit/Player can pick from any land.
/// </summary>
public class SpiritPicksLandAction : IActOn<Spirit> {

	public SpiritPicksLandAction( IActOn<TargetSpaceCtx> spaceAction, string andInToForPreposition ) { 
		_spaceAction = spaceAction;
		_landPreposition = andInToForPreposition;
	}

	public string Description => $"{_spaceAction.Description} {_landPreposition} {_diffString}{LandCriteria.Description}";

	public bool IsApplicable( Spirit spirit ) => true;

	public async Task ActAsync( Spirit self ) {

		for(int i = 0; i < _landsPerSpirit; ++i) {

			var preFiltered = GameState.Current.Spaces
				.Where( x => !_disallowedSpaces.Contains( x.Space ) ) // for picking Different spaces
				.Select( s => self.Target( s.Space ) )
				.Where( _spaceAction.IsApplicable );  // Matches action Criteria

			var spaceOptions = LandCriteria.Filter( preFiltered ).ToArray();
			if(spaceOptions.Length == 0) return;

			Space space = _firstPickTokenClasses != null
				? await PickSpaceBySelectingToken( self, spaceOptions )
				: await self.SelectSpaceAsync( "Select space to " + _spaceAction.Description, spaceOptions.Select( x => x.Space ), _present );

			if(space == null) return;

			if(_chooseDifferentLands)
				_disallowedSpaces.Add( space );

			await _spaceAction.ActAsync( self.Target(space) );
		}
	}

	#region Configue Methods

	public SpiritPicksLandAction Which( TargetSpaceCtxFilter filter ) { _landCriteria = filter; return this; }
	public SpiritPicksLandAction MakeOptional() { _present = Present.Done; return this; }
	public SpiritPicksLandAction AllDifferent() {  _chooseDifferentLands = true; return this; }
	public SpiritPicksLandAction ByPickingToken( params ITokenClass[] tokenClasses ) { _firstPickTokenClasses = tokenClasses; return this; }
	public SpiritPicksLandAction EachSpiritPicks( int count ) { _landsPerSpirit = count; return this; }

	#endregion

	#region private

	async Task<Space> PickSpaceBySelectingToken( Spirit self, TargetSpaceCtx[] spaceOptions ) {
		// Get options
		IEnumerable<SpaceToken> GetSpaceTokens( TargetSpaceCtx x ) => x.Tokens.SpaceTokensOfAnyTag( _firstPickTokenClasses );
		SpaceToken[] spaceTokenOptions = spaceOptions.SelectMany( GetSpaceTokens ).ToArray();

		// Select
		SpaceToken st = await self.SelectAsync( new A.SpaceToken( "Select token for " + _spaceAction.Description, spaceTokenOptions, Present.Always ) );
		self.PreSelect(st);

		return st?.Space;
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
	ITokenClass[] _firstPickTokenClasses;
	int _landsPerSpirit = 1;
	#endregion
}
