using Microsoft.CodeAnalysis;

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
public class ConfigurableTestFixture {

	static readonly public GameBuilder GameBuilder = new GameBuilder(
		new Basegame.GameComponentProvider(),
		new BranchAndClaw.GameComponentProvider(),
		new FeatherAndFlame.GameComponentProvider(),
		new JaggedEarth.GameComponentProvider()
	);

	#region Configurable Parts

	public SpiritPresence Presence => Spirit.Presence;

	public Spirit Spirit {
		get => _spirit ??= new RiverSurges();
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

		var gameConfig = new GameConfiguration().ConfigSpirits( RiverSurges.Name ).ConfigBoards( "A" );
		adjustCfg(gameConfig);

		_gameState = GameBuilder.BuildGame( gameConfig );
		_spirit = _gameState.Spirits.Single();
		_board = _gameState.Island.Boards.Single();
	}

	#endregion

	/// <summary>
	/// Inits elements in list. (Does not modify elements not in list.)
	/// </summary>
	public void InitElements(string elementString ) => Spirit.Configure().Elements(elementString);

	public TargetSpiritCtx TargetSelf => Spirit.Target( Spirit );

	public void InitPresence( SpaceSpec space, int count ) {
		Spirit.Given_IsOn( GameState.Tokens[space], count );
	}

	public void InitTokens( SpaceSpec space, string tokenString ) {
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

	public void ChoosePush( IToken token, SpaceSpec destination ) {
		NextDecision.Choose( token.Text ); // passing Text in case options are actually SpaceTokens
		NextDecision.Choose( destination );
	}

	public ConfigurableTestFixture IsActive( Task task ) {
		task.IsCompleted.ShouldBeFalse();
		return this;
	}

	#endregion

	#region Canned Tests

	public void VerifyEnergyTrack( int revealedSpaces, int expectedEnergyGrowth, string elements ) {
		Spirit.Presence.Energy.Given_SlotsRevealed( revealedSpaces );
		Spirit.EnergyPerTurn.ShouldBe( expectedEnergyGrowth );
		Spirit.Elements.Summary(false).ShouldBe( elements );
	}

	/// <summary>
	/// Operates strictly with the Presence tracks.
	/// </summary>
	public void VerifyCardTrack( int revealedSpaces, int expectedCardPlayCount, string elements ) {
		Spirit.Presence.CardPlays.Given_SlotsRevealed( revealedSpaces );
		Spirit.NumberOfCardsPlayablePerTurn.ShouldBe( expectedCardPlayCount );
		Spirit.Elements.Summary(false).ShouldBe( elements );
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

static public class GameConfiguration_Extensions {
	/// <summary>
	/// Uses ConfigurableTestFixture to construct the game.
	/// </summary>
	static public GameState BuildGame(this GameConfiguration cfg) => ConfigurableTestFixture.GameBuilder.BuildGame(cfg);
}