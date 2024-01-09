namespace SpiritIsland.Tests;

//Setup:
//	Spirit / Presence / Cards / Prepared Elements / Energy
//	Board / Tokens
//	Power Cards( Major / Minor)
//	Invader Cards
//	Fear Cards
//	Blight Card

//	Fear Count
//	Blighted Island state

//Testing Power Cards
//	Contained / Data - changes state
//	Behavior
//		Move things in and out of a space.
//		Things that add temporary events handlers.  ( when this happens, then that behaves differently )

/// <summary>
/// Lazy inits all parts, allowing use to pre-configure parts they care about, before they are lazy-inited.
/// </summary>
public class ConfigurableTestFixture : IHaveHealthPenaltyPerStrife {

	#region Configurable Parts

	public PresenceTrack EnergyTrack {
		get => _energyTrack ??= new PresenceTrack( Track.Energy1, Track.Energy2, Track.Energy3 );
		set => Init( ref _energyTrack, value, nameof(_energyTrack));
	}
	PresenceTrack _energyTrack;

	public PresenceTrack CardPlayTrack {
		get => _cardTrack ??= new PresenceTrack( Track.Card1, Track.Card2, Track.Card3 );
		set => Init( ref _cardTrack, value, nameof(_cardTrack) );
	}
	PresenceTrack _cardTrack;

	public SpiritPresence Presence => Spirit.Presence;

	public Spirit Spirit {
		get => _spirit ??= new ConfigurableSpirit( s => new SpiritPresence(s,EnergyTrack,CardPlayTrack) );
		set {
			Init( ref _spirit, value, nameof(_spirit) );
		}
	} 
	Spirit _spirit;

	public Board Board {
		get => _board ??= Board.BuildBoardA();
		set => Init( ref _board, value, nameof(_board) );
	} 
	Board _board;

	public GameState GameState => _gameState ??= new GameState( Spirit, Board );
	GameState _gameState;

	/// <summary>
	/// Uses Configuration to Init the GameState, Spirit, and Board
	/// </summary>
	public void InitConfiguration(Action<GameConfiguration> adjustCfg) {
		if(_spirit != null) throw new InvalidOperationException( "InitConfiguration must be called before Spirit is initialized." );
		if(_board != null) throw new InvalidOperationException( "InitConfiguration must be called before Board is initialized." );
		if(_gameState != null) throw new InvalidOperationException( "InitConfiguration must be called before GameState is initialized." );

		var gameConfig = new GameConfiguration().SetSpirits( RiverSurges.Name ).SetBoards( "A" );
		adjustCfg(gameConfig);

		_gameState = GameBuilder.BuildGame( gameConfig );
		_spirit = _gameState.Spirits.Single();
		_board = _gameState.Island.Boards.Single();
	}

	static readonly public GameBuilder GameBuilder;

	static ConfigurableTestFixture() {
		var providers = new IGameComponentProvider[] {
			new SpiritIsland.Basegame.GameComponentProvider(),
			new SpiritIsland.BranchAndClaw.GameComponentProvider(),
			new SpiritIsland.FeatherAndFlame.GameComponentProvider(),
			new SpiritIsland.JaggedEarth.GameComponentProvider(),
		};
		GameBuilder = new GameBuilder( providers );
	}
	#endregion

	/// <summary>
	/// Inits elements in list. (Does not modify elements not in list.)
	/// </summary>
	public void InitElements(string elementString ) => Spirit.Configure().Elements(elementString);

	public TargetSpiritCtx TargetSelf => Spirit.Target( Spirit );

	public void InitPresence( Space space, int count ) {
		var spaceState = GameState.Tokens[space];
		var dif = count - spaceState[Presence.Token];
		SpiritExtensions.Given_Setup( Presence, spaceState, dif );
	}

	public void InitTokens( Space space, string tokenString ) {
		GameState.Tokens[space].Given_HasTokens( tokenString );
	}

	public int HealthPenaltyPerStrife { get; set; }

	public void InitRavageCard( InvaderCard card ) {
		card.Flipped = true;
		GameState.InvaderDeck.Ravage.Cards.Add(card);
	}

	#region Choose

	public void Choose( string choiceText ) => NextDecision.Choose( choiceText );
	public void Choose( IOption choice ) => NextDecision.Choose( choice.Text );
	public DecisionContext NextDecision => Spirit.NextDecision();

	public string FormatOptions => Spirit.Portal.Next.FormatOptions();

	public void ChoosePush( IToken token, Space destination ) {
		NextDecision.Choose( token.Text ); // passing Text in case options are actually SpaceTokens
		NextDecision.Choose( destination );
	}

	public ConfigurableTestFixture IsActive( Task task ) {
		task.IsCompleted.ShouldBeFalse();
		return this;
	}

	#endregion

	#region Canned Tests

	static async Task TakeFromTrack( int revealedSpaces, SpiritPresence presence, IPresenceTrack track ) {
		for(int i = 1; i < revealedSpaces; i++)
			await track.RevealOptions.First().RemoveAsync( presence.Token );
	}

	public async Task VerifyEnergyTrack( int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		await TakeFromTrack( revealedSpaces, Spirit.Presence, Spirit.Presence.Energy ).ShouldComplete( "Taking from Energy Track");
		Spirit.EnergyPerTurn.ShouldBe( expectedEnergyGrowth );
		Spirit.Elements.BuildElementString(false).ShouldBe( elements );
	}

	/// <summary>
	/// Operates strictly with the Presence tracks.
	/// </summary>
	public async Task VerifyCardTrack( int revealedSpaces, int expectedCardPlayCount, string elements ) {
		await TakeFromTrack( revealedSpaces, Spirit.Presence, Spirit.Presence.CardPlays ).ShouldComplete( "Taking from Card Track" );
		Spirit.NumberOfCardsPlayablePerTurn.ShouldBe( expectedCardPlayCount );
		Spirit.Elements.BuildElementString(false).ShouldBe( elements );
	}

	public void VerifyReclaim1Count( int count ) {
		ConfigurableTestFixture fix = this;
		fix.Spirit.Presence.RevealedActions.OfType<ReclaimN>().Count().ShouldBe( count );
	}

	#endregion

	#region private

	static void Init<T>( ref T storage, T newValue, string varName ) {
		if(storage != null) 
			throw new InvalidOperationException( $"{varName} is already set to an object of type {storage.GetType().Name}" );
		storage = newValue;
	}

	#endregion
}
