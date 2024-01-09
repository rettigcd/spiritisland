namespace SpiritIsland.Tests.Spirits.BringerNS;

[Collection("BaseGame Spirits")]
public class DreadApparitions_Tests {

	public DreadApparitions_Tests() {}

	[Fact]
	public async Task DirectFear_GeneratesDefend() {
		Bringer spirit = new Bringer();
		Board board = Board.BuildBoardA();
		Space a5 = board[5];
		_ = new GameState( spirit, board );

		// Given: DA run
		await spirit.When_TargetingSpace( a5, DreadApparitions.ActAsync );

		// When: generating 2 fear
		await spirit.When_TargetingSpace( a5, ctx=> ctx.AddFear(2) );

		// Then: also get 3 defend
		a5.Tokens.Defend.Count.ShouldBe( 2+1 );// +1 from D.A.
	}

	[Fact]
	public async Task TownDamage_Generates2Defend() {

		Bringer spirit = new Bringer();
		Board board = Board.BuildBoardA();
		Space a5 = board[5];
		GameState gs = new GameState( spirit, board );
		gs.DisableBlightEffect();

		// Given: 
		a5.Given_HasTokens("1T@2");
		await spirit.When_TargetingSpace( a5, DreadApparitions.ActAsync );

		// When: destroying the town with Power
		await spirit.When_TargetingSpace( a5, async ctx => await ctx.Invaders.DestroyNOfClass( 1, Human.Town ), (u) => {
			u.NextDecision.HasPrompt( "Push T@2 to" ).Choose( "A4" );
		} );

		// Then: 2 fear should have triggered 2 defend
		a5.Tokens.Defend.Count.ShouldBe( 2+1 );

	}

	// Generate 5 DATD fear by 'killing' a city - should defend 5
	[Fact]
	public async Task CityDamage_Generates5Defend() {
		Spirit spirit = new Bringer();
		Board board = Board.BuildBoardA();
		Space a5 = board[5];
		GameState gs = new GameState( spirit, board );
		gs.DisableBlightEffect();

		// Given
		a5.Given_HasTokens("1C@3");
		spirit.Given_HasPresenceOn(a5);
		//  And
		await spirit.When_TargetingSpace( a5, DreadApparitions.ActAsync );

		// When: destroying city
		await spirit.When_TargetingSpace(a5, ctx => ctx.Invaders.DestroyNOfClass( 1, Human.City ) );

		// Then: 5 fear should have triggered 2 defend
		board[5].Tokens.Defend.Count.ShouldBe( 5+1 );// Dread Apparitions has 1 fear

	}

	[Fact]
	public async Task DahanDamage_Generates0() {

		Bringer spirit = new Bringer();
		Board board = Board.BuildBoardA();
		Space a5 = board[5];
		GameState gs = new GameState( spirit, board );
		SpaceState tokens = a5.Tokens;
		gs.DisableBlightEffect();
		gs.IslandWontBlight();


		// Given: has 1 city and lots of dahan
		tokens.Given_HasTokens("1C@3,10D@2");
		spirit.Presence.Given_Setup(tokens,1);
		//   And: used Dread Apparitions
		await spirit.When_TargetingSpace( a5, DreadApparitions.ActAsync );

		// When: dahan destroy the city
		await tokens.Space.When_Ravaging();

		// Then: 2 fear from city
		SpiritIsland.Fear fear = gs.Fear;
		int actualFear = fear.EarnedFear + 4 * fear.ActivatedCards.Count;
		actualFear.ShouldBe(2+1);// normal (1 from Dread Apparitions)
		//  And: 1 defend bonus
		tokens.Defend.Count.ShouldBe( 1 ); // from dread apparitions
	}

	[Fact]
	public async Task FearInOtherLand_Generates0() {
		Bringer spirit = new Bringer();
		Board board = Board.BuildBoardA(); Space a5 = board[5];
		_ = new GameState( spirit, board );

		// Given: has 1 city and lots of dahan
		a5.Tokens.Given_HasTokens("1C@3,10D@2");
		//   And: triggered DA
		await spirit.When_TargetingSpace( a5, DreadApparitions.ActAsync );

		// When: Power causes fear in a different land
		await spirit.When_TargetingSpace( a5, ctx => board[1].Tokens.AddFear(6) );

		// Then: no defend bonus
		board[5].Tokens.Defend.Count.ShouldBe( 1+0 );// 1=>Dread Apparitions
	}

}