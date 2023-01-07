namespace SpiritIsland;

/// <summary>
/// Wraps: Space, Token-Counts on that space, API to publish token-changed events.
/// </summary>
public class SpaceState : HasNeighbors<SpaceState> {

	#region constructor

	public SpaceState( Space space, CountDictionary<Token> counts, IIslandTokenApi tokenApi, GameState gameState ) {
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

	public int this[Token specific] {
		get {
			ValidateNotDead( specific );
			int count = _counts[specific];
			if( specific is UniqueToken ut )
				count += _api.GetDynamicTokensFor(this, ut);
			return count;
		}
		protected set {
			ValidateNotDead( specific );
			_counts[specific] = value; 
		}
	}

	public IEnumerable<Token> Keys => _counts.Keys; // !! This won't list virtual (defend) tokens

	#region Enumeration / Detection(HaS) / Summing

	protected IEnumerable<Token> OfCategoryInternal( TokenCategory category ) => Keys.Where( k => k.Class.Category == category );
	protected IEnumerable<Token> OfClassInternal( TokenClass tokenClass ) => Keys.Where( x => x.Class == tokenClass );
	protected IEnumerable<Token> OfAnyClassInternal( params TokenClass[] classes ) => Keys.Where( specific => classes.Contains( specific.Class ) );

	public Token[] OfCategory( TokenCategory category ) => OfCategoryInternal( category ).ToArray();
	public Token[] OfClass( TokenClass tokenClass ) => OfClassInternal( tokenClass ).ToArray();
	public Token[] OfAnyClass( params TokenClass[] classes ) => OfAnyClassInternal( classes ).ToArray();
	public HealthToken[] OfHealthClass( HealthTokenClass tokenClass ) => OfClassInternal( tokenClass ).Cast<HealthToken>().ToArray();
	public HealthToken[] OfAnyHealthClass( params HealthTokenClass[] classes ) => OfAnyClassInternal( classes ).Cast<HealthToken>().ToArray();

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
	public TokenBindingNoEvents Beasts => new ( this, TokenType.Beast );
	public TokenBindingNoEvents Disease => new ( this, TokenType.Disease );
	public TokenBindingNoEvents Wilds => new ( this, TokenType.Wilds );
	public TokenBindingNoEvents Badlands => new ( this, TokenType.Badlands ); // This should not be used directly from inside Actions
	public HealthTokenClassBinding_NoEvents Dahan => new HealthTokenClassBinding_NoEvents( this, TokenType.Dahan );
	//public HealthTokenClassBinding_NoEvents Explorers => new HealthTokenClassBinding_NoEvents( this, Invader.Explorer );
	//public HealthTokenClassBinding_NoEvents Towns => new HealthTokenClassBinding_NoEvents( this, Invader.Town );
	//public HealthTokenClassBinding_NoEvents Cities => new HealthTokenClassBinding_NoEvents( this, Invader.City );

	#endregion

	#region private

	static void ValidateNotDead( Token specific ) {
		if(specific is HealthToken ht && ht.RemainingHealth == 0) 
			throw new ArgumentException( "We don't store dead counts" );
	}

	public readonly CountDictionary<Token> _counts; // !!! public for Tokens_ForIsland Memento, create own momento.
	protected readonly IIslandTokenApi _api;
	protected readonly GameState _gameState; // !! merge this usage into token api, I guess.  Here for access to island.

	#endregion

	#region Non-event Generationg Token Changes

	/// <summary> Non-event-triggering setup </summary>
	public void Adjust( Token specific, int delta ) {
		if(specific is HealthToken ht && ht.RemainingHealth == 0) 
			throw new System.ArgumentException( "Don't try to track dead tokens." );
		_counts[specific] += delta;
	}

	public void InitDefault( HealthTokenClass tokenClass, int value )
		=> Init( GetDefault( tokenClass ), value );

	public void AdjustDefault( HealthTokenClass tokenClass, int delta ) 
		=> Adjust( GetDefault( tokenClass ), delta );

	public void Init( Token specific, int value ) {
		_counts[specific] = value;
	}

	public HealthToken GetDefault( HealthTokenClass tokenClass ) => this._api.GetDefault( tokenClass );

	public void ReplaceAllWith( Token original, Token replacement ) {
		Adjust( replacement, this[original] );
		Init( original, 0 );
	}
	public void ReplaceNWith( int countToReplace, Token oldToken, Token newToken ) {
		countToReplace = Math.Min( countToReplace, this[oldToken] );
		Adjust( oldToken, -countToReplace );
		Adjust( newToken, countToReplace );
	}

	#endregion


	#region Event-Generating Token Changes

	public GameState AccessGameState() => _api.AccessGameState();

	#endregion

	#region Invader Specific

	/// <summary> Includes dreaming invaders. </summary>
	public IEnumerable<HealthToken> InvaderTokens() => OfCategory( TokenCategory.Invader ).Cast<HealthToken>();

	public bool HasInvaders() => Has( TokenCategory.Invader );

	public bool HasStrife => Keys.OfType<HealthToken>().Any(x=>x.StrifeCount>0);

	public int CountStrife() => Keys.OfType<HealthToken>().Where(x=>x.StrifeCount>0).Sum( t => _counts[t] );

	public int TownsAndCitiesCount() => this.SumAny( Invader.Town_City );

	public int InvaderTotal() => InvaderTokens().Sum( i => _counts[i] );

	#endregion

	public HealthToken RemoveStrife( HealthToken orig, int tokenCount ) {
		HealthToken lessStrifed = orig.AddStrife( -1 );
		this[lessStrifed] += tokenCount;
		this[orig] -= tokenCount;
		return lessStrifed;
	}

	public int AttackDamageFrom1( HealthToken ht ) => ht.Class.Category == TokenCategory.Dahan 
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

	// This is trying to accomplish: (Some terrain other than Ocean)
	public IEnumerable<SpaceState> CascadingBlightOptions => Adjacent
		 .Where(x => !this._gameState.Island.Terrain_ForBlight.MatchesTerrain(x, Terrain.Ocean) // normal case,
			|| this._gameState.Island.Terrain_ForBlight.MatchesTerrain( x, Terrain.Wetland ) );

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

	public void Skip1Ravage( string label ) => Adjust( new SkipRavage( label ), 1 );

	// !!! can we have 2 of these tokens?, will it throw an exception?, will it stop 2 builds?
	public void Skip1Build( string label ) => Adjust( SkipBuild.Default( label ), 1 );

	public void Skip1Explore( string label ) => Adjust( new SkipExploreTo( label ), 1 );

	public void SkipAllBuilds( string label, params TokenClass[] stoppedClasses ) {
		if(stoppedClasses == null || stoppedClasses.Length == 0)
			stoppedClasses = Invader.Town_City;
		Adjust( new SkipBuild( label, UsageDuration.SkipAllThisTurn, stoppedClasses ), 1 );
	}

	#endregion


	public void TimePasses() {
		Blight.Blocked = false; // !!! move inside cleanup token???

		foreach( var cleanup in Keys.OfType<TokenWithEndOfRoundCleanup>().ToArray() )
			cleanup.EndOfRoundCleanup( this );
	}

	#region Ravage

	public Task Ravage() {
		GameState gameState = AccessGameState();
		RavageBehavior cfg = gameState.GetRavageConfiguration( Space );
		return cfg.Exec( this, gameState );
	}

	#endregion

	// Utter a Curse of Dread and Bone requires these overrides
	public override bool Equals( object obj ) => obj is SpaceState other && other.Space == Space;
	public override int GetHashCode() => Space.GetHashCode();


	// ! To ensure we don't side-step any Spirit Powers, All calls to this for Spirit Powers
	// should go through SelfCtx / TargetSpaceCtx / TargetSpiritCtx
	// Calls for Invader actions, Adversaries, etc, can call direclty.
	public virtual ActionableSpaceState Bind( UnitOfWork actionScope ) => new ActionableSpaceState( this, actionScope );
}
