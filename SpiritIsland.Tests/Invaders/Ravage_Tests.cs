using SpiritIsland.Invaders.Ravage;

namespace SpiritIsland.Tests.Invaders;

public class Ravage_Tests {

	[Fact]
	public async Task ExchangesFor_1E() {
		var gs = new SoloGameState(Boards.A);
		var space = gs.Board[1].ScopeSpace;
		var exchanges = LogRavageExchanges(gs);

		// Given: 1 explorer
		space.InitDefault(Human.Explorer,1);

		// When: ravaging
		await space.Ravage();//.ShouldComplete("ravage");

		// Then: have only 1 exchange - attacker
		exchanges.Count.ShouldBe(1);
		exchanges[0].ToString().ShouldBe( "attack: (1E@1) deal 1 damage to defenders ([none]) destroying 0 of them." );
	}

	[Fact]
	public async Task Normal_1E1D() {
		var gs = new SoloGameState(Boards.A);
		var exchanges = LogRavageExchanges(gs);
		var space = gs.Board[1].ScopeSpace;

		// Given: 1 explorer, 1 Dahan
		space.InitDefault( Human.Explorer, 1 );
		space.InitDefault( Human.Dahan, 1 );

		// When: ravaging
		await space.Ravage();//.ShouldComplete("ravage");

		// Then: have only 1 exchange - attacker
		exchanges.Count.ShouldBe( 2 );
		exchanges[0].ToString().ShouldBe( "attack: (1E@1) deal 1 damage to defenders (1D@2) destroying 0 of them." );
		exchanges[1].ToString().ShouldBe( "defend: (1D@1) deal 2 damage, leaving [none]." );
	}

	[Fact]
	public async Task Ambush_2T2D() {
		var gs = new SoloGameState(Boards.A);
		var exchanges = LogRavageExchanges(gs);
		var space = gs.Board[1].ScopeSpace;

		// Given: 2 towns, 2 dahan - 1 regular and 1 ambush, and 1 vitality
		space.InitDefault( Human.Town, 2 );
		space.InitDefault( Human.Dahan, 1 );
		space.Init( new HumanToken( Human.Dahan, 2 ).SetRavageOrder(RavageOrder.Ambush), 1 );
		space.Init( Token.Vitality, 1 ); // to prevent blight

		// When: ravaging
		Task ravage = space.Ravage();
		await ravage;
		

		// Then: have only 1 exchange - attacker
		exchanges.Count.ShouldBe( 3 );
		exchanges[0].ToString().ShouldBe( "ambush: (1D@2) deal 2 damage, leaving 1T@2." );
		exchanges[1].ToString().ShouldBe( "attack: (1T@2) deal 2 damage to defenders (1D@2,1D@2) destroying 1 of them." );
		exchanges[2].ToString().ShouldBe( "defend: (1D@2) deal 2 damage, leaving [none]." );
	}

	[Fact]
	public async Task NoDahan_StrifeComesOff() {
		var gs = new SoloGameState(new RiverSurges(), Boards.A);
		var space = gs.Board[1].ScopeSpace;

		// Given: no dahan to fight.

		//   And: 1 explorer with strife
		space.Init( space.GetDefault(Human.Explorer).AddStrife(1),1);

		// When: ravaging
		await space.Ravage();//.ShouldComplete("ravage");

		// Then: strife is gone
		space.Summary.ShouldBe("1E@1");
	}

	[Fact]
	public async Task IncludesDefendTokenVarieties() {
		var gs = new SoloGameState(new RiverSurges(), Boards.A);
		var space = gs.Board[1].ScopeSpace;

		// Given: 1 dahan & 1 town
		space.Given_InitSummary("1D@2,1T@2");

		//   And: 1 variety defend
		TokenVariety defendToken = new TokenVariety(Token.Defend, "💧");
		space.Init(defendToken, 1);

		// When: ravaging
		await space.Ravage();//.ShouldComplete("ravage");

		// Then: strife is gone
		space.Summary.ShouldBe("1D@1,1G-💧");
	}

	static List<RavageExchange> LogRavageExchanges(GameState gs) {
		List<RavageExchange> exchanges = [];
		gs.NewLogEntry += (e) => { if( e is Log.RavageEntry re ) exchanges.AddRange(re.Exchange); };
		return exchanges;
	}

}
