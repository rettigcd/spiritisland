namespace SpiritIsland;

/// <summary>
/// Spirit/Player picks 1 land from their home board.
/// </summary>
public class NLandsPerBoard : IExecuteOn<BoardCtx> {

	#region constructor

	public NLandsPerBoard( IExecuteOn<TargetSpaceCtx> spaceAction, string preposition, int count ) {
		_spaceAction = spaceAction;
		_preposition = preposition;
		_count = count;
	}

	#endregion

	#region Configure

	public NLandsPerBoard Which(TargetSpaceCtxFilter spaceFilter ) { _landCriteria = spaceFilter; return this; }
	public NLandsPerBoard ByPickingToken( params TokenClass[] tokenClasses ) { 
		_tokenFactory = GetTokensMatchingClass;
		_firstPickTokenClasses = tokenClasses; 
		return this;
	}

	public NLandsPerBoard ByPickingToken( Func<TargetSpaceCtx, IEnumerable<IToken>> tokenFactory ) {
		_tokenFactory = tokenFactory;
		_firstPickTokenClasses = null; // not usedtokenClasses;
		return this;
	}


	#endregion

	#region simple public properties
	public string Description => _spaceAction.Description + " " + _preposition + " " + LandCriteria.Description;
	public bool IsApplicable( BoardCtx ctx ) => true;
	#endregion

	public async Task Execute( BoardCtx ctx ) {

		var used = new List<Space>();

		// !!! although filtering by the Action Criteria is helpful most of the time,
		// There might be instances when players wants to pick the no-op action.
		// Therefore we might not want to filter based on the action.
		// Maybe pass both spaces to UI so UI can determine which options to make available.

		for(int i=0; i<_count; i++ ) {

			var spaceOptions = ctx.Board.Spaces
				.Except( used )
				.Select( ctx.Target )
				.Where( x => x.IsInPlay )           // layer 2 filter
				.Where( _spaceAction.IsApplicable )  // Matches action criteria  (Can't act on items that aren't there)
				.Where( LandCriteria.Filter )        // Matches custom space - criteria
				.ToArray();
			if(spaceOptions.Length == 0) return;

			TargetSpaceCtx spaceCtx = _tokenFactory != null
				? await PickSpaceBySelectingToken( ctx, spaceOptions )
				: await ctx.SelectSpace( "Select space to " + _spaceAction.Description, spaceOptions.Select( x => x.Space ), Present.Always );

			used.Add( spaceCtx.Space );

			await _spaceAction.Execute( spaceCtx );
		}
	}

	#region private

	IEnumerable<IToken> GetTokensMatchingClass( TargetSpaceCtx x ) => x.Tokens.OfAnyClass( _firstPickTokenClasses );

	async Task<TargetSpaceCtx> PickSpaceBySelectingToken( SelfCtx ctx, TargetSpaceCtx[] spaceOptions ) {

		// Get options
		Func<TargetSpaceCtx,IEnumerable<IToken>> tokenFactory = GetTokensMatchingClass;
		SpaceToken[] spaceTokenOptions = spaceOptions
			.SelectMany( x => 
				tokenFactory(x)
					.Cast<IVisibleToken>()
					.Select( t => new SpaceToken( x.Space, t ) )
			)
			.ToArray();

		// Select
		SpaceToken st = await ctx.Self.Gateway.Decision( new Select.TokenFromManySpaces( "Select token for " + _spaceAction.Description, spaceTokenOptions, Present.Always ) );

		if(st != null) {
			ctx.Self.Gateway.Preloaded = st;
			return ctx.Target( st.Space );
		}

		// !!! Bug - We need to Load into the Preloaded property a 'no-choice/null' option so the auto-picker knows to pick nothing.
		return null;

	}


	readonly IExecuteOn<TargetSpaceCtx> _spaceAction;
	readonly string _preposition;
	TargetSpaceCtxFilter LandCriteria => _landCriteria ??= Is.AnyLand;
	TargetSpaceCtxFilter _landCriteria;
	readonly int _count;
	TokenClass[] _firstPickTokenClasses;
	Func<TargetSpaceCtx, IEnumerable<IToken>> _tokenFactory;

	#endregion

}