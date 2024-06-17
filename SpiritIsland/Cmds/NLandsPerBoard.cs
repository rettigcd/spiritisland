namespace SpiritIsland;

/// <summary>
/// Spirit/Player picks 1 land from their home board.
/// </summary>
public class NLandsPerBoard( IActOn<TargetSpaceCtx> _spaceAction, string _preposition, int _count ) 
	: IActOn<BoardCtx>
{

	#region constructor

	#endregion

	#region Configure

	public NLandsPerBoard Which( CtxFilter<TargetSpaceCtx> spaceFilter ) { _landCriteria = spaceFilter; return this; }
	public NLandsPerBoard ByPickingToken( params ITokenClass[] tokenClasses ) { 
		_tokenFactory = GetTokensMatchingClass;
		_firstPickTokenClasses = tokenClasses; 
		return this;
	}

	public NLandsPerBoard ByPickingToken( Func<TargetSpaceCtx, IEnumerable<ISpaceEntity>> tokenFactory ) {
		_tokenFactory = tokenFactory;
		_firstPickTokenClasses = null; // not usedtokenClasses;
		return this;
	}


	#endregion

	#region simple public properties
	public string Description => $"{_spaceAction.Description} {_preposition} {LandCriteria.Description}.";
	public bool IsApplicable( BoardCtx ctx ) => true;
	#endregion

	public async Task ActAsync( BoardCtx ctx ) {

		var used = new List<SpaceSpec>();

		// Although filtering by the Action Criteria is helpful most of the time,
		// There might be instances when players wants to pick the no-op action.
		// In those cases, do NOT use the .Filtering
		for(int i=0; i<_count; i++ ) {

			var preFiltered = ctx.Board.Spaces
				.Except( used )
				.Select( ctx.Target )
				.Where( _spaceAction.IsApplicable )  // Matches action criteria  (Can't act on items that aren't there)
				.ToArray();
			var filtered1 = LandCriteria.Filter( preFiltered ).ToArray();
			if(preFiltered.Length == 0) return;

			Space space = _tokenFactory != null
				? await PickSpaceBySelectingToken( ctx.Self, filtered1 )
				: await ctx.Self.SelectSpaceAsync( "Select space to " + _spaceAction.Description, filtered1.Select( x => x.Space ), Present.Always );

			if(space == null) return; // no matching tokens

			used.Add( space.SpaceSpec );

			await _spaceAction.ActAsync( ctx.Target(space) );
		}
	}

	#region private

	IEnumerable<IToken> GetTokensMatchingClass( TargetSpaceCtx x ) => x.Space.OfAnyTag( _firstPickTokenClasses );

	async Task<Space> PickSpaceBySelectingToken( Spirit self, TargetSpaceCtx[] spaceOptions ) {

		// Get options
		Func<TargetSpaceCtx,IEnumerable<ISpaceEntity>> tokenFactory = GetTokensMatchingClass;
		SpaceToken[] spaceTokenOptions = spaceOptions
			.SelectMany( x => tokenFactory(x).Cast<IToken>().On(x.Space) )
			.ToArray();

		// Select
		SpaceToken st = await self.SelectAsync( new A.SpaceTokenDecision( "Select token for " + _spaceAction.Description, spaceTokenOptions, Present.Always ) );
		self.PreSelect(st); // recording null is fine because when it probably means no space matches criteria and user won't be given an option anyway.

		return st?.Space;
	}

	CtxFilter<TargetSpaceCtx> LandCriteria => _landCriteria ??= Is.AnyLand;
	CtxFilter<TargetSpaceCtx> _landCriteria;
	ITokenClass[] _firstPickTokenClasses;
	Func<TargetSpaceCtx, IEnumerable<ISpaceEntity>> _tokenFactory;

	#endregion

}