using System.Text.RegularExpressions;

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

	public SpiritPresence Presence => _presence ??= new SpiritPresence(EnergyTrack,CardPlayTrack);
	SpiritPresence _presence;

	public Spirit Spirit {
		get => _spirit ??= new ConfigurableSpirit( Presence );
		set {
			if(_presence != null) throw new InvalidOperationException("Cannot set the spirit when the Presence has already been initialized.");
			Init( ref _spirit, value, nameof(_spirit) );
			_presence = _spirit.Presence;
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

		var providers = new IGameComponentProvider[] {
			new SpiritIsland.Basegame.GameComponentProvider(),
			new SpiritIsland.BranchAndClaw.GameComponentProvider(),
			new SpiritIsland.PromoPack1.GameComponentProvider(),
			new SpiritIsland.JaggedEarth.GameComponentProvider(),
		};
		_gameState = new GameBuilder( providers ).BuildGame( gameConfig );
		_spirit = _gameState.Spirits.Single();
		_board = _gameState.Island.Boards.Single();
	}

	#endregion

	/// <summary>
	/// Inits elements in list. (Does not modify elements not in list.)
	/// </summary>
	public void InitElements(string elementString ) => Spirit.Configure().Elements(elementString);

	public SelfCtx SelfCtx {
//		get => _selfCtx ??= Spirit.BindMyPower( GameState, GameState.StartAction( ActionCategory.Spirit_Power ) );//??? is it ok to spin up actions like this?
		get {
			if( _selfCtx == null )
				_selfCtx = Spirit.BindMyPowers( GameState, GameState.StartAction( ActionCategory.Spirit_Power ) );
			return _selfCtx;
		}
		set => _selfCtx = value;
	}

	SelfCtx _selfCtx;

	public TargetSpiritCtx TargetSelf => SelfCtx.TargetSpirit( Spirit );

	public void InitPresence( Space space, int count ) {
		var spaceState = GameState.Tokens[space];
		var dif = count - Presence.CountOn( spaceState );
		Presence.Adjust( spaceState, dif );
	}

	public void InitTokens( Space space, string tokenString ) {
		GameState.Tokens[space].InitTokens( tokenString );
	}

	public int HealthPenaltyPerStrife { get; set; }

	public void InitRavageCard( IInvaderCard card ) {
		GameState.InvaderDeck.Ravage.Cards.Add(card);
	}

	#region Choose

	public void Choose( string choiceText ) => NextDecision.Choose( choiceText );
	public void Choose( IOption choice ) => NextDecision.Choose( choice );
	public DecisionContext NextDecision => Spirit.NextDecision();

	public string FormatOptions => Spirit.Gateway.Next.FormatOptions();

	public void ChoosePush( Token token, Space destination ) {
		NextDecision.Choose( token );
		NextDecision.Choose( destination );
	}

	public ConfigurableTestFixture IsActive( Task task ) {
		task.IsCompleted.ShouldBeFalse();
		return this;
	}

	#endregion

	#region Canned Tests

	public async Task VerifyEnergyTrack( int revealedSpaces, int expectedEnergyGrowth, string elements ) {

		var presence = Spirit.Presence;
		var track = presence.Energy;

		for(int i = 1; i < revealedSpaces; i++) {
			var choice = track.RevealOptions.First();
			await presence.TakeFrom( choice, GameState );
		}
		Spirit.EnergyPerTurn.ShouldBe( expectedEnergyGrowth );
		Spirit.Elements.BuildElementString(false).ShouldBe( elements );
	}

	/// <summary>
	/// Operates strictly with the Presence tracks.
	/// </summary>
	public async Task VerifyCardTrack( int revealedSpaces, int expectedCardPlayCount, string elements ) {
		var presence = Spirit.Presence;
		var track = presence.CardPlays;

		for(int i = 1; i < revealedSpaces; i++) {
			var choice = track.RevealOptions.First();
			await presence.TakeFrom( choice, GameState );
		}
		Spirit.NumberOfCardsPlayablePerTurn.ShouldBe( expectedCardPlayCount );
		Spirit.Elements.BuildElementString(false).ShouldBe( elements );
	}

	public void VerifyReclaim1Count( int count ) {
		ConfigurableTestFixture fix = this;
		var trackActions = fix.Spirit.Presence.RevealedActions.OfType<ReclaimN>().ToArray();
		trackActions.Length.ShouldBe( count );
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
