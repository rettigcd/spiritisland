namespace SpiritIsland.Tests.Invaders; 

public class Ravage_Tests {

	readonly Spirit _spirit;
	readonly Board _board;
	readonly SpaceState _tokens;
	readonly GameState _gs;
	readonly List<RavageExchange> _exchanges = new List<RavageExchange>();

	public Ravage_Tests() {
		_spirit = new RiverSurges();
		_board = Board.BuildBoardA();
		_gs = new GameState( _spirit, _board );
		_gs.NewLogEntry += _gs_NewLogEntry;
		// Do this after GameState has been initialized so that ActionScopes are initialized.
		_tokens = _board[1].Tokens;
	}

	void _gs_NewLogEntry( Log.ILogEntry e ) {
		if( e is Log.RavageEntry re)
			_exchanges.AddRange( re.Exchange );
	}

	[Fact]
	public async Task ExchangesFor_1E() {

		// Given: 1 explorer
		_tokens.InitDefault(Human.Explorer,1);

		// When: ravaging
		await _tokens.Ravage();//.ShouldComplete("ravage");

		// Then: have only 1 exchange - attacker
		_exchanges.Count.ShouldBe(1);
		_exchanges[0].ToString().ShouldBe( "attack: (1E@1) deal 1 damage to defenders ([none]) destroying 0 of them." );
	}

	[Fact]
	public async Task Normal_1E1D() {
		// Given: 1 explorer, 1 Dahan
		_tokens.InitDefault( Human.Explorer, 1 );
		_tokens.InitDefault( Human.Dahan, 1 );

		// When: ravaging
		await _tokens.Ravage();//.ShouldComplete("ravage");

		// Then: have only 1 exchange - attacker
		_exchanges.Count.ShouldBe( 2 );
		_exchanges[0].ToString().ShouldBe( "attack: (1E@1) deal 1 damage to defenders (1D@2) destroying 0 of them." );
		_exchanges[1].ToString().ShouldBe( "defend: (1D@1) deal 2 damage, leaving [none]." );
	}

	[Fact]
	public async Task Ambush_2T2D() {
		// Given: 2 towns, 2 dahan - 1 regular and 1 ambush, and 1 vitality
		_tokens.InitDefault( Human.Town, 2 );
		_tokens.InitDefault( Human.Dahan, 1 );
		_tokens.Init( new HumanToken( Human.Dahan, 2 ).SetRavageOrder(RavageOrder.Ambush), 1 );
		_tokens.Init( Token.Vitality, 1 ); // to prevent blight

		// When: ravaging
		Task ravage = _tokens.Ravage();
		await ravage;
		

		// Then: have only 1 exchange - attacker
		_exchanges.Count.ShouldBe( 3 );
		_exchanges[0].ToString().ShouldBe( "ambush: (1D@2) deal 2 damage, leaving 1T@2." );
		_exchanges[1].ToString().ShouldBe( "attack: (1T@2) deal 2 damage to defenders (1D@2,1D@2) destroying 1 of them." );
		_exchanges[2].ToString().ShouldBe( "defend: (1D@2) deal 2 damage, leaving [none]." );
	}


}
