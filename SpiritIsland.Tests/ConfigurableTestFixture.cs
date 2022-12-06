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

		var gameConfig = new GameConfiguration {
			SpiritType = typeof(RiverSurges),
			Board = "A",
			Color = "Red",
		};
		adjustCfg(gameConfig);

		var providers = new IGameComponentProvider[] {
			new SpiritIsland.Basegame.GameComponentProvider(),
			new SpiritIsland.BranchAndClaw.GameComponentProvider(),
			new SpiritIsland.PromoPack1.GameComponentProvider(),
			new SpiritIsland.JaggedEarth.GameComponentProvider(),
		};
		_gameState = gameConfig.BuildGame( providers );
		_spirit = _gameState.Spirits.Single();
		_board = _gameState.Island.Boards.Single();
	}

	#endregion

	/// <summary>
	/// Inits elements in list. (Does not modify elements not in list.)
	/// </summary>
	public void InitElements(string elementString ) {
		var counts = ElementCounts.Parse( elementString );
		foreach(var el in counts.Keys)
			Spirit.Elements[el] = counts[el];
	}

	public SelfCtx SelfCtx {
		get => _selfCtx ??= Spirit.BindMyPower( GameState, GameState.StartAction( ActionCategory.Default ) );//??? is it ok to spin up actions like this?
		set => _selfCtx = value;
	}
	SelfCtx _selfCtx;

	public TargetSpiritCtx TargetSelf => SelfCtx.TargetSpirit( Spirit );

	public void InitTokens( Space space, string tokenString ) {
		var tokens = GameState.Tokens[space];
		foreach(var part in tokenString.Split( ',' )) {
			var (token,count) = ParseToken( part );
			tokens.Adjust( count, token );
		}
	}

	public void InitPresence( Space space , int count ) {
		var spaceState = SelfCtx.GameState.Tokens[space];
		var dif = count - Presence.CountOn(spaceState);
		Presence.Adjust( spaceState, dif );
	}

	static readonly Regex tokenParser = new Regex( @"(\d+)(\w)@(\d+)(\^*)" );
	(int,Token) ParseToken( string part ) {
		var match = tokenParser.Match( part );
		if(!match.Success) throw new FormatException( $"Unrecognized token [{part}] Example: 1T@2." );
		var tokenClass = match.Groups[2].Value switch {
			"C" => Invader.City,
			"T" => Invader.Town,
			"E" => Invader.Explorer,
			"D" => TokenType.Dahan,
			_ => throw new Exception( $"Invader Initial [{match.Groups[2].Value}]" ),
		};
		int fullHealth = tokenClass.Attack; // hack - class doesn't have full health, so we will cheat and use attack as the full health.
		var token = new HealthToken( 
			tokenClass,
			this,
			fullHealth,
			fullHealth - int.Parse( match.Groups[3].Value ), // damage
			match.Groups[4].Value.Length // strife
		);
		int count = int.Parse( match.Groups[1].Value );
		return (count, token);
	}

	public int HealthPenaltyPerStrife { get; set; }

	public void InitRavageCard( IInvaderCard card ) {
		GameState.InvaderDeck.Ravage.Cards.Add(card);
	}

	#region Choose

	public void Choose(string choiceText ) {
		var decision = Spirit.Gateway.GetCurrent( true );
		if(decision == null) throw new Exception("no Decision presented.");
		var matchingChoices = decision.Options
			.Where(o=>o.Text.ToLower().Contains( choiceText.ToLower() ))
			.ToArray();
		switch( matchingChoices.Length) {
			case 0: throw new Exception($"No option contains '{choiceText}' in: "+FormatDecision(decision));
			case 1: Spirit.Gateway.Choose( matchingChoices[0] ); return;
			default: throw new Exception( $"Multiple option contain '{choiceText}' in: " + FormatDecision( decision ) );
		}
	}

	public void Choose( IOption choice ) {
		var decision = Spirit.Gateway.GetCurrent( true );
		if(decision == null) throw new Exception( "no Decision presented." );
		if( !decision.Options.Contains( choice )) {
			throw new Exception( $"{choice.Text} not found in: " + FormatDecision( decision ) );
		}
		Spirit.Gateway.Choose( choice ); return;
	}

	public IOption[] ChoiceOptions{ get {
		var decision = Spirit.Gateway.GetCurrent( true );
			return decision != null ? decision.Options : throw new Exception( "no Decision presented." );
	} }

	public void ChoosePush( Token token, Space destination ) {
		Choose( token );
		Choose( destination );
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

	static string FormatDecision( IDecision d ) { return d.Prompt + ":" + d.Options.Select( x => x.Text ).Join( ", " ); }

	static void Init<T>( ref T storage, T newValue, string varName ) {
		if(storage != null) 
			throw new InvalidOperationException( $"{varName} is already set to an object of type {storage.GetType().Name}" );
		storage = newValue;
	}

	#endregion
}

class SpaceSpecificInvaderCard : IInvaderCard {
	readonly Space _space;
	public SpaceSpecificInvaderCard(Space space) { _space = space; }

	public int InvaderStage => 1;
	public string Text => throw new NotImplementedException();

	public bool Skip { get; set; }
	public bool HoldBack { get; set; }

	public Task Build( GameState gameState ) => throw new NotImplementedException();
	public Task Explore( GameState gameState ) => throw new NotImplementedException();
	public bool Matches( Space space ) => space == _space;
	public Task Ravage( GameState gameState ) => throw new NotImplementedException();
}