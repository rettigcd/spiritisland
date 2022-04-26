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
public class ConfigurableTestFixture {

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
		set => Init( ref _spirit, value, nameof(_spirit) );
	} 
	Spirit _spirit;

	public Board Board {
		get => _board ??= Board.BuildBoardA();
		set => Init( ref _board, value, nameof(_board) );
	} 
	Board _board;

	public GameState GameState => _gameState ??= new GameState( Spirit, Board );
	GameState _gameState;

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
		get => _selfCtx ??= Spirit.BindMyPower( GameState );
		set => _selfCtx = value;
	}
	SelfCtx _selfCtx;

	public TargetSpiritCtx TargetSelf => Spirit.BindMyPower( GameState ).TargetSpirit( Spirit );

	static readonly Regex tokenParser = new Regex( @"(\d+)(\w)@(\d+)" );
	public void InitTokens( Space space, string tokenString ) {
		var tokens = GameState.Tokens[space];
		foreach(var part in tokenString.Split( ',' )) {
			var (token,count) = ParseToken( part );
			tokens.Adjust( count, token );
		}
	}

	public void InitPresence( Space space , int count ) {
		var dif = count - Presence.CountOn(space);
		Presence.Adjust( space, dif );
	}

	static (int,Token) ParseToken( string part ) {
		var match = tokenParser.Match( part );
		if(!match.Success) throw new FormatException( $"Unrecognized token [{part}]." );
		var tokenClass = match.Groups[2].Value switch {
			"C" => Invader.City,
			"T" => Invader.Town,
			"E" => Invader.Explorer,
			"D" => TokenType.Dahan,
			_ => throw new Exception( $"Invader Initial [{match.Groups[2].Value}]" ),
		};
		var token = new HealthToken( tokenClass, tokenClass.Attack, tokenClass.Attack - int.Parse( match.Groups[3].Value ) );// Ignoring strife...
		int count = int.Parse( match.Groups[1].Value );
		return (count, token);
	}

	#region Choose

	public void Choose(string choiceText ) {
		var decision = Spirit.Action.GetCurrent( true );
		if(decision == null) throw new Exception("no Decision presented.");
		var matchingChoices = decision.Options
			.Where(o=>o.Text.ToLower().Contains( choiceText.ToLower() ))
			.ToArray();
		switch( matchingChoices.Length) {
			case 0: throw new Exception($"No option contains '{choiceText}' in: "+FormatDecision(decision));
			case 1: Spirit.Action.Choose( matchingChoices[0] ); return;
			default: throw new Exception( $"Multiple option contain '{choiceText}' in: " + FormatDecision( decision ) );
		}
	}

	public void Choose( IOption choice ) {
		var decision = Spirit.Action.GetCurrent( true );
		if(decision == null) throw new Exception( "no Decision presented." );
		if( !decision.Options.Contains( choice )) {
			throw new Exception( $"{choice.Text} not found in: " + FormatDecision( decision ) );
		}
		Spirit.Action.Choose( choice ); return;
	}

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
		Spirit.Elements.BuildElementString( "," ).ShouldBe( elements );
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
		Spirit.Elements.BuildElementString( "," ).ShouldBe( elements );
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
