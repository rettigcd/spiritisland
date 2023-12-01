namespace SpiritIsland;

/// <summary>
/// Spirit/Player picks 1 land from their home board.
/// </summary>
public class NLandsPerBoard : IActOn<BoardCtx> {

	#region constructor

	public NLandsPerBoard( IActOn<TargetSpaceCtx> spaceAction, string preposition, int count ) {
		_spaceAction = spaceAction;
		_preposition = preposition;
		_count = count;
	}

	#endregion

	#region Configure

	public NLandsPerBoard Which(TargetSpaceCtxFilter spaceFilter ) { _landCriteria = spaceFilter; return this; }
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
	public string Description => _spaceAction.Description + " " + _preposition + " " + LandCriteria.Description;
	public bool IsApplicable( BoardCtx ctx ) => true;
	#endregion

	public async Task ActAsync( BoardCtx ctx ) {

		var used = new List<Space>();

		// Although filtering by the Action Criteria is helpful most of the time,
		// There might be instances when players wants to pick the no-op action.
		// In those cases, do NOT use the .Filtering
		for(int i=0; i<_count; i++ ) {

			var spaceOptions = ctx.Board.Spaces
				.Except( used )
				.Select( ctx.Target )
				.Where( _spaceAction.IsApplicable )  // Matches action criteria  (Can't act on items that aren't there)
				.Where( LandCriteria.Filter )        // Matches custom space - criteria
				.ToArray();
			if(spaceOptions.Length == 0) return;

			Space space = _tokenFactory != null
				? await PickSpaceBySelectingToken( ctx.Self, spaceOptions )
				: await ctx.Self.SelectSpaceAsync( "Select space to " + _spaceAction.Description, spaceOptions.Select( x => x.Space ), Present.Always );

			if(space == null) return; // no matching tokens

			used.Add( space );

			await _spaceAction.ActAsync( ctx.Target(space) );
		}
	}

	#region private

	IEnumerable<IToken> GetTokensMatchingClass( TargetSpaceCtx x ) => x.Tokens.OfAnyTag( _firstPickTokenClasses );

	async Task<Space> PickSpaceBySelectingToken( Spirit self, TargetSpaceCtx[] spaceOptions ) {

		// Get options
		Func<TargetSpaceCtx,IEnumerable<ISpaceEntity>> tokenFactory = GetTokensMatchingClass;
		SpaceToken[] spaceTokenOptions = spaceOptions
			.SelectMany( x => tokenFactory(x).Cast<IToken>().On(x.Space) )
			.ToArray();

		// Select
		SpaceToken st = await self.SelectAsync( new A.SpaceToken( "Select token for " + _spaceAction.Description, spaceTokenOptions, Present.Always ) );
		self.PreSelect(st); // recording null is fine because when it probably means no space matches criteria and user won't be given an option anyway.

		return st?.Space;
	}


	readonly IActOn<TargetSpaceCtx> _spaceAction;
	readonly string _preposition;
	TargetSpaceCtxFilter LandCriteria => _landCriteria ??= Is.AnyLand;
	TargetSpaceCtxFilter _landCriteria;
	readonly int _count;
	ITokenClass[] _firstPickTokenClasses;
	Func<TargetSpaceCtx, IEnumerable<ISpaceEntity>> _tokenFactory;

	#endregion

}