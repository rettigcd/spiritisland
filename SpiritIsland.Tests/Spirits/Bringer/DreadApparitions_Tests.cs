namespace SpiritIsland.Tests.Spirits.BringerNS;

public class DreadApparitions_Tests {

	Board board;
	TargetSpaceCtx ctx;

	ActionScope Init() {
		Bringer spirit = new Bringer();
		board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board );
		var scope = ActionScope.Start_NoStartActions( ActionCategory.Spirit_Power );
		ctx = spirit.BindMyPowers().Target( board[5] );
		return scope;
	}

	[Fact]
	public async Task DirectFear_GeneratesDefend() {
		await using var x = Init();

		async Task When() {
			// Given: using Dread Apparitions
			await DreadApparitions.ActAsync( ctx );
			// When: generating 2 fear
			ctx.AddFear( 2 );
		}
		_ = When();

		Assert_DefenceIs( 2+1 ); // +1 from D.A.
	}

	void Assert_DefenceIs(int expectedDefence) {
		ctx.Tokens.Defend.Count.ShouldBe( expectedDefence );
	}

	[Fact]
	public async Task TownDamage_Generates2Defend() {
		await using var x = Init();

		// Disable destroying presence
		ctx.GameState.DisableBlightEffect();


		// has town
		ctx.Tokens.AdjustDefault( Human.Town, 1 );

		async Task When() {
			// Given: using Dread Apparitions
			await DreadApparitions.ActAsync( ctx );
			// When: destroying the town
			await ctx.Invaders.DestroyNOfClass(1,Human.Town);
		}
		_ = When();

		// Then: 2 fear should have triggered 2 defend
		Assert_DefenceIs( 2+1 ); // +1 Dread App...

	}

	// Generate 5 DATD fear by 'killing' a city - should defend 5
	[Fact]
	public async Task CityDamage_Generates5Defend() {
		await using var x = Init();

		// Disable destroying presence
		ctx.GameState.DisableBlightEffect();

		// has city
		ctx.Tokens.AdjustDefault( Human.City, 1 );

		await DreadApparitions.ActAsync( ctx );
		// When: destroying the city
		await ctx.Invaders.DestroyNOfClass( 1, Human.City );

		// Then: 5 fear should have triggered 2 defend
		Assert_DefenceIs( 5+1 ); // Dread Apparitions has 1 fear

	}

	[Fact]
	public async Task DahanDamage_Generates0() {

		Bringer spirit = new Bringer();
		var board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board );
		var tokens = board[5].Tokens;

		// Disable destroying presence
		gs.DisableBlightEffect();
		gs.IslandWontBlight();

		string startingGuid = ActionScope.Current.Id.ToString();

		// has 1 city and lots of dahan
		tokens.AdjustDefault( Human.City, 1 ); // don't use ctx.Invaders because it has a fake/dream invader count
		tokens.Dahan.Init(10);

		// Given: using Dread Apparitions
		async Task DoIt(){
			await using var myScope = await ActionScope.Start(ActionCategory.Spirit_Power);
			string powerGuid = ActionScope.Current.Id.ToString();
			var ctx = spirit.BindMyPowers().Target( board[5] );
			await DreadApparitions.ActAsync( ctx );
		}

		await DoIt();

		var postActionGuid = ActionScope.Current.Id.ToString();
		postActionGuid.ShouldBe(startingGuid);

		// When: dahan destroy the city
		await tokens.Ravage();

		// Then: 2 fear from city
		// Assert_GeneratedFear(2+1); // normal (1 from Dread Apparitions)
		var fear = gs.Fear;
		int actualFear = fear.EarnedFear + 4 * fear.ActivatedCards.Count;
		actualFear.ShouldBe(2+1);

		// and 1 defend bonus
		tokens.Defend.Count.ShouldBe( 1 ); // from dread apparitions
	}

	[Fact]
	public void FearInOtherLand_Generates0() {
		Init();

		// has 1 city and lots of dahan
		ctx.Tokens.AdjustDefault( Human.City, 1 );
		ctx.Dahan.Init( 10 );

		async Task When() {
			// Given: using Dread Apparitions
			await DreadApparitions.ActAsync( ctx );

			// When: Power causes fear in a different land
			ctx.GameState.Fear.AddDirect(new FearArgs( 6 ) { space = board[1] } );
		}
		_ = When();

		// but no defend bonus
		Assert_DefenceIs( 1 ); // 1=>Dread Apparitions

	}

}