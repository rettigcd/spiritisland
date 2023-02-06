namespace SpiritIsland;

/// <summary>
/// Wraps: Space, Token-Counts on that space, API to publish token-changed events.
/// Has same Scope as GameState (not bound to an ActionScope
/// </summary>
public class SpaceState : HasNeighbors<SpaceState> {

	#region constructor

	public SpaceState( Space space, CountDictionary<IToken> counts, IIslandTokenApi tokenApi, GameState gameState ) {
		Space = space;
		_counts = counts;
		_api = tokenApi;
		_gameState = gameState;
	}

	/// <summary> Clone / copy constructor </summary>
	protected SpaceState( SpaceState src ) {
		this.Space = src.Space;
		_counts = src._counts;
		_api = src._api;
		_gameState = src._gameState;
	}

	#endregion

	public Space Space { get; }

	public BoardState Board => new BoardState( Space.Board, _gameState );

	public int this[IToken specific] {
		get {
			ValidateNotDead( specific );
			int count = _counts[specific];
			if( specific is UniqueToken ut )
				count += _api.GetDynamicTokensFor(this, ut);
			return count;
		}
	}

	public IEnumerable<IToken> Keys => _counts.Keys; // !! This won't list virtual (defend) tokens

	#region Enumeration / Detection(HaS) / Summing

	public IToken[] OfCategory( TokenCategory category ) => OfCategoryInternal( category ).ToArray();
	public IToken[] OfClass( TokenClass tokenClass ) => OfClassInternal( tokenClass ).ToArray();
	public IToken[] OfAnyClass( params TokenClass[] classes ) => OfAnyClassInternal( classes ).ToArray(); // !! This could *probably* return IVisibleToken

	public HumanToken[] OfHumanClass( HumanTokenClass tokenClass ) => OfClassInternal( tokenClass ).Cast<HumanToken>().ToArray();
	public HumanToken[] OfAnyHumanClass( params HumanTokenClass[] classes ) => OfAnyClassInternal( classes ).Cast<HumanToken>().ToArray();

	// 2 SpaceToken helper classes
	public IEnumerable<SpaceToken> SpaceTokensOfClass( TokenClass tokenClass ) 
		=> OfClassInternal( tokenClass ).OfType<IVisibleToken>().Select(x=>new SpaceToken(Space,x));
	public IEnumerable<SpaceToken> SpaceTokensOfAnyClass( params TokenClass[] tokenClasses )
		=> OfAnyClassInternal( tokenClasses ).OfType<IVisibleToken>().Select( x => new SpaceToken( Space, x ) );

	protected IEnumerable<IToken> OfCategoryInternal( TokenCategory category ) => Keys.Where( k => k.Class.Category == category );
	protected IEnumerable<IToken> OfClassInternal( TokenClass tokenClass ) => Keys.Where( x => x.Class == tokenClass );
	protected IEnumerable<IToken> OfAnyClassInternal( params TokenClass[] classes ) => Keys.Where( specific => classes.Contains( specific.Class ) );

	public bool Has( TokenClass inv ) => OfClassInternal( inv ).Any();
	public bool Has( TokenCategory cat ) => OfCategoryInternal( cat ).Any();
	public bool HasAny( params TokenClass[] healthyInvaders ) => OfAnyClassInternal( healthyInvaders ).Any();

	public int Sum( TokenClass tokenClass ) => OfClassInternal( tokenClass ).Sum( k => _counts[k] );
	public int Sum( TokenCategory category ) => OfCategoryInternal( category ).Sum( k => _counts[k] );
	public int SumAny( params TokenClass[] healthyInvaders ) => OfAnyClassInternal( healthyInvaders ).Sum( k => _counts[k] );

	#endregion

	#region To-String methods

	/// <summary>Gets all tokens that have a SpaceAbreviation</summary>
	public string Summary => _counts.TokenSummary();

	public override string ToString() => Space.Label + ":" + Summary;

	#endregion

	#region Token-Type Sub-groups

	public virtual BlightTokenBindingNoEvents Blight => new BlightTokenBindingNoEvents( this );
	public IDefendTokenBinding Defend => new DefendTokenBinding( this );
	public BeastBinding_NoEvents Beasts => new ( this, Token.Beast );
	public TokenBindingNoEvents Disease => new ( this, Token.Disease );
	public TokenBindingNoEvents Wilds => new ( this, Token.Wilds );
	public TokenBindingNoEvents Badlands => new ( this, Token.Badlands ); // This should not be used directly from inside Actions
	public HealthTokenClassBinding_NoEvents Dahan => new HealthTokenClassBinding_NoEvents( this, Human.Dahan );
	//public HealthTokenClassBinding_NoEvents Explorers => new HealthTokenClassBinding_NoEvents( this, Invader.Explorer );
	//public HealthTokenClassBinding_NoEvents Towns => new HealthTokenClassBinding_NoEvents( this, Invader.Town );
	//public HealthTokenClassBinding_NoEvents Cities => new HealthTokenClassBinding_NoEvents( this, Invader.City );

	#endregion

	#region private

	static void ValidateNotDead( IToken specific ) {
		if(specific is HumanToken ht && ht.RemainingHealth == 0) 
			throw new ArgumentException( "We don't store dead counts" );
	}

	public readonly CountDictionary<IToken> _counts; // !!! public for Tokens_ForIsland Memento, create own momento.
	protected readonly IIslandTokenApi _api;
	protected readonly GameState _gameState; // !! merge this usage into token api, I guess.  Here for access to island.

	#endregion

	#region Non-event Generationg Token Changes

	/// <summary> Non-event-triggering setup </summary>
	public void Adjust( IToken specific, int delta ) {
		if(specific is HumanToken human && human.RemainingHealth == 0) 
			throw new System.ArgumentException( "Don't try to track dead tokens." );
		if(specific is ITrackMySpaces selfTracker) AdjustTrackedToken( selfTracker, delta);
		_counts[specific] += delta;
	}

	public void Init( IToken specific, int newValue ) {
		int old = _counts[specific];
		Adjust( specific, newValue-old ); // go through Adjust so that we keep ITrackMySpaces in sync
	}

	public void InitDefault( HumanTokenClass tokenClass, int value )
		=> Init( GetDefault( tokenClass ), value );

	public void AdjustDefault( HumanTokenClass tokenClass, int delta ) 
		=> Adjust( GetDefault( tokenClass ), delta );

	public HumanToken GetDefault( HumanTokenClass tokenClass ) => this._api.GetDefault( tokenClass );

	public void ReplaceAllWith( IToken original, IToken replacement ) {
		Adjust( replacement, this[original] );
		Init( original, 0 );
	}
	public void ReplaceNWith( int countToReplace, IToken oldToken, IToken newToken ) {
		countToReplace = Math.Min( countToReplace, this[oldToken] );
		Adjust( oldToken, -countToReplace );
		Adjust( newToken, countToReplace );
	}

	#endregion

	#region Event-Generating Token Changes

	public GameState AccessGameState() => _gameState;

	#endregion

	#region Invader Specific

	/// <summary> Includes dreaming invaders. </summary>
	public IEnumerable<HumanToken> InvaderTokens() => OfCategory( TokenCategory.Invader ).Cast<HumanToken>();

	public bool HasInvaders() => Has( TokenCategory.Invader );

	public bool HasStrife => Keys.OfType<HumanToken>().Any(x=>0<x.StrifeCount);
	public int StrifeCount => Keys.OfType<HumanToken>().Sum( x => x.StrifeCount );

	public int CountStrife() => Keys.OfType<HumanToken>().Where(x=>x.StrifeCount>0).Sum( t => _counts[t] );

	public int TownsAndCitiesCount() => this.SumAny( Human.Town_City );

	public int InvaderTotal() => InvaderTokens().Sum( i => _counts[i] );

	#endregion

	public HumanToken RemoveStrife( HumanToken orig, int tokenCount ) {
		HumanToken lessStrifed = orig.AddStrife( -1 );
		Adjust(lessStrifed, tokenCount);
		Adjust(orig, -tokenCount);
		return lessStrifed;
	}

	public int AttackDamageFrom1( HumanToken ht ) => ht.Class.Category == TokenCategory.Dahan 
		? 2
		: _api.InvaderAttack( ht.Class );

	#region Adjacent Properties

	public IEnumerable<SpaceState> Adjacent { get {
		foreach(var space in Space.Adjacent)
			yield return _gameState.Tokens[space];

		if(LinkedViaWays != null && !LinkedViaWays.Space.InStasis)
			yield return LinkedViaWays;
	} }

	public SpaceState LinkedViaWays; // HACK - for Finder

	public IEnumerable<SpaceState> Range(int maxDistance) => this.CalcDistances( maxDistance ).Keys;

	/// <summary> Explicitly named so not to confuse with Powers - Range commands. </summary>
	public IEnumerable<SpaceState> InOrAdjacentTo => Range( 1 );

	#endregion

	#region Skip API

	public void SkipAllInvaderActions( string label ) {
		Skip1Ravage( label );
		Skip1Build( label );
		Skip1Explore( label );
	}

	public void Skip1Ravage( string _ ) => Adjust( new SkipRavage(), 1 );

	public void Skip1Build( string label ) => Adjust( SkipBuild.Default( label ), 1 );

	public void Skip1Explore( string _ ) => Adjust( new SkipExploreTo(), 1 );

	public void SkipAllBuilds( string label, params TokenClass[] stoppedClasses ) {
		if(stoppedClasses == null || stoppedClasses.Length == 0)
			stoppedClasses = Human.Town_City;
		Adjust( new SkipBuild( label, UsageDuration.SkipAllThisTurn, stoppedClasses ), 1 );
	}

	#endregion

	#region Ravage

	public Task Ravage() {
		GameState gameState = AccessGameState();
		RavageBehavior cfg = gameState.GetRavageConfiguration( Space );
		return cfg.Exec( this, gameState );
	}

	#endregion

	#region Ocean Helpers

	public void AdjustTrackedToken( ITrackMySpaces token, int delta ) {
//		token.Adjust( Space, delta );
		_api.Adjust(token,Space,delta);
	}

	public bool HasTokenOnBoard( ITrackMySpaces token ) {
//		return token.IsOn( Space.Board );
		return _api.IsOn(token,Space.Board);
	}

	#endregion

	public void TimePasses() {
		foreach(var cleanup in Keys.OfType<ITokenWithEndOfRoundCleanup>().ToArray())
			cleanup.EndOfRoundCleanup( this );
	}

	// Utter a Curse of Dread and Bone requires these overrides
	public override bool Equals( object obj ) => obj is SpaceState other && other.Space == Space;
	public override int GetHashCode() => Space.GetHashCode();


	// ! To ensure we don't side-step any Spirit Powers, All calls to this for Spirit Powers
	// should go through SelfCtx / TargetSpaceCtx / TargetSpiritCtx
	// Calls for Invader actions, Adversaries, etc, can call direclty.
	public virtual ActionableSpaceState Bind( UnitOfWork actionScope ) => new ActionableSpaceState( this, actionScope );
}
