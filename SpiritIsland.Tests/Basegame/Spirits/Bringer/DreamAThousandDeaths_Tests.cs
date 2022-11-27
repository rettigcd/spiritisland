namespace SpiritIsland.Tests.Basegame.Spirits.BringerNS;

public class DreamAThousandDeaths_Tests {

	readonly Board board;
	readonly GameState gs;
	readonly TargetSpaceCtx ctx;
	readonly Spirit spirit;
	readonly VirtualUser User;
	readonly UnitOfWork unitOfWork;

	public DreamAThousandDeaths_Tests(){
		spirit = new Bringer();
		User = new VirtualUser(spirit);
		board = Board.BuildBoardA();
		gs = new GameState( spirit, board );
		unitOfWork = gs.StartAction();
		ctx = MakeFreshPowerCtx();

		// Disable destroying presence
//			ctx.GameState.AddBlightSideEffect = (gs,space) => new AddBlightEffect { Cascade=false,DestroyPresence=false };
		ctx.GameState.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );

	}

	TargetSpaceCtx MakeFreshPowerCtx() {
		var ctx = spirit.BindMyPower(gs, unitOfWork ); // This is correct usage.
		return ctx.Target( board[5] );
	}

	// 1: Raging Storm - 1 damage to each invader (slow)
	static readonly Func<TargetSpaceCtx,Task> OneDamageToEachAsync = RagingStorm.ActAsync;
	// 1 & 2: The Jungle Hugers - destroy all explorers and all towns
	static readonly Func<TargetSpaceCtx, Task> DestroyAllExplorersAndTownsAsync = TheJungleHungers.ActAsync;
	// 4, cleansing flood - 4 damage (+10 if you hvae 4 water)
	static readonly Func<TargetSpaceCtx, Task> FourDamage = CleansingFloods.ActAsync;

	[Theory]
	[InlineData( "damage" )]
	[InlineData( "destroy" )]
	public void DreamKilledExplorers_ArePushed(string method) {
		const int count = 2;

		// Given: 2 explorers
		ctx.Tokens.AdjustDefault(Invader.Explorer, count );

		// When: causing 1 damage to each invader
		switch(method) {
			case "damage": _ = OneDamageToEachAsync( ctx ); break;
			case "destroy": _ = DestroyAllExplorersAndTownsAsync( ctx ); break;
		}

		// Then: dream-death allows User pushes them
		for(int i = 0; i < count; ++i)
			User.PushesTokensTo( "E@1", "A1,A4,A6,(A7),A8" );

		// And: 0-fear
		Assert_GeneratedFear( 0 );

		//  and: explorer on destination
		ctx.GameState.Assert_Invaders( board[7], $"{count}E@1" );
		//  and: not at origin
		ctx.GameState.Assert_Invaders( board[5], $"" );
	}

	[Fact]
	public void KillingTown_GeneratesFear_PushesIt() {
		int count = 2;
		// generate 2 fear per town destroyed,
		// pushes town

		// Given: 2 town
		ctx.Tokens.AdjustDefault( Invader.Town, count );

		// When: destroying towns
		_ = DestroyAllExplorersAndTownsAsync( ctx );

		// Then: dream-death allows User pushes them
		for(int i = 0; i < count; ++i)
			User.PushesTokensTo("T@2","A1,A4,A6,(A7),A8" );

		// And:4-fear
		Assert_GeneratedFear( count * 2 );

		//  and: town on destination
		ctx.GameState.Assert_Invaders( board[7], $"{count}T@2" );
		//  and: not at origin
		ctx.GameState.Assert_Invaders( board[5], $"" );

	}

	[Fact]
	public void DreamDamageResetsEachPower() {

		// Given: 2 explorers
		ctx.Tokens.AdjustDefault( Invader.City, 1 );

		// When: 3 separate actinos cause 1 damage
		async Task Run3Async() {
			await OneDamageToEachAsync( MakeFreshPowerCtx() );
			await OneDamageToEachAsync( MakeFreshPowerCtx() );
			await OneDamageToEachAsync( MakeFreshPowerCtx() );
		}
		_ = Run3Async();

		User.Assert_Done();

		// And: 0-fear
		Assert_GeneratedFear( 0 ); // city never destroyed
	}

	[Fact]
	public async Task ConsecutivePowersCanDreamKillMultipletimes() {

		// Given: 1 very-damaged city
		ctx.Tokens.Adjust( StdTokens.City1, 1 );

		// When: 3 separate actinos cause 1 damage
		// EACH power gets a fresh ctx so INVADERS can reset
		await OneDamageToEachAsync( MakeFreshPowerCtx() ); 
		await OneDamageToEachAsync( MakeFreshPowerCtx() );
		await OneDamageToEachAsync( MakeFreshPowerCtx() );

		User.Assert_Done();

		// And: 0-fear
		Assert_GeneratedFear( 3*5 ); // city never destroyed
		// City still there
		ctx.Invaders.Tokens[StdTokens.City1 ].ShouldBe(1);
	}

	[Fact]
	public void MaxKillOnce() {
		// Given: 1 very-damaged city
		ctx.Tokens.Adjust( StdTokens.City1, 1 );

		// When: doing 4 points of damage
		async Task PlayCard() { try { await FourDamage( MakeFreshPowerCtx() ); } catch( Exception ex) {
			_ = ex.ToString();
		} }
		_ = PlayCard();

		User.SelectsDamageRecipient(4,"C@1");

		// And: 0-fear
		Assert_GeneratedFear( 1 * 5 ); // city only destroyed once

		// City with partial damage still there
		ctx.Invaders[StdTokens.City1 ].ShouldBe( 1 );
	}

	void Assert_GeneratedFear( int expectedFearCount ) {
		int actualGeneratedFear = ctx.GameState.Fear.EarnedFear
			+ 4 * ctx.GameState.Fear.ActivatedCards.Count;
		actualGeneratedFear.ShouldBe(expectedFearCount,"fear countis wrong");
	}

}
