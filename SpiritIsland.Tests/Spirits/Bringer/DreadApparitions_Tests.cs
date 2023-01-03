namespace SpiritIsland.Tests.Spirits.BringerNS;

public class DreadApparitions_Tests {

	readonly Board board;
	readonly TargetSpaceCtx ctx;

	public DreadApparitions_Tests() {
		Bringer spirit = new Bringer();
		board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board );
		var action = gs.StartAction( ActionCategory.Spirit_Power ); // !! not disposing
		ctx = spirit.BindMyPowers( gs, action ).Target( board[5] );
	}

	[Fact]
	public void DirectFear_GeneratesDefend() {

		async Task When() {
			// Given: using Dread Apparitions
			await DreadApparitions.ActAsync( ctx );
			// When: generating 2 fear
			ctx.AddFear( 2 );
		}
		_ = When();

		Assert_DefenceIs( 2+1 ); // +1 from D.A.
	}

	private void Assert_DefenceIs(int expectedDefence) {
		ctx.Tokens.Defend.Count.ShouldBe( expectedDefence );
	}

	[Fact]
	public void TownDamage_Generates2Defend() {

		// Disable destroying presence
		ctx.GameState.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );


		// has town
		ctx.Tokens.AdjustDefault(Invader.Town, 1);

		async Task When() {
			// Given: using Dread Apparitions
			await DreadApparitions.ActAsync( ctx );
			// When: destroying the town
			await ctx.Invaders.Destroy(1,Invader.Town);
		}
		_ = When();

		// Then: 2 fear should have triggered 2 defend
		Assert_DefenceIs( 2+1 ); // +1 Dread App...

	}

	// Generate 5 DATD fear by 'killing' a city - should defend 5
	[Fact]
	public async Task CityDamage_Generates5Defend() {
		// Disable destroying presence
		ctx.GameState.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );

		// has city
		ctx.Tokens.AdjustDefault( Invader.City, 1 );

		await DreadApparitions.ActAsync( ctx );
		// When: destroying the city
		await ctx.Invaders.Destroy( 1, Invader.City );

		// Then: 5 fear should have triggered 2 defend
		Assert_DefenceIs( 5+1 ); // Dread Apparitions has 1 fear

	}

	[Fact]
	public async Task DahanDamage_Generates0() {

		// Disable destroying presence
		ctx.GameState.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );


		// has 1 city and lots of dahan
		ctx.Tokens.AdjustDefault( Invader.City, 1 ); // don't use ctx.Invaders because it has a fake/dream invader count
		ctx.Dahan.Init(10);

		// Given: using Dread Apparitions
		await DreadApparitions.ActAsync( ctx );

		// When: dahan destroy the city
		await ctx.Space.DoARavage( ctx.GameState );

		// Then: 2 fear from city
		Assert_GeneratedFear(2+1); // normal (1 from Dread Apparitions)
		// and 1 defend bonus
		Assert_DefenceIs( 1 ); // from dread apparitions

	}

	[Fact]
	public void FearInOtherLand_Generates0() {
		// has 1 city and lots of dahan
		ctx.Tokens.AdjustDefault( Invader.City, 1 );
		ctx.Dahan.Init( 10 );

		async Task When() {
			// Given: using Dread Apparitions
			await DreadApparitions.ActAsync( ctx );

			// When: Power causes fear in a different land
			ctx.GameState.Fear.AddDirect(new FearArgs { space = board[1], FromDestroyedInvaders = false, count = 6 } );
		}
		_ = When();

		// but no defend bonus
		Assert_DefenceIs( 1 ); // 1=>Dread Apparitions

	}


	void Assert_GeneratedFear( int expectedFearCount ) {

		int actualFear = ctx.GameState.Fear.EarnedFear
			+ 4 * ctx.GameState.Fear.ActivatedCards.Count;

		actualFear.ShouldBe(expectedFearCount,"fear count is wrong");

	}

}