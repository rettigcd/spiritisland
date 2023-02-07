namespace SpiritIsland.Tests.Spirits.BringerNS;

[Trait("SpecialRule","ToDreamAThousandDeaths")]
public class ToDreamAThousandDeaths_Tests {

	readonly Board _board;
	readonly GameState _gameState;
	readonly TargetSpaceCtx _ctx;
	readonly Spirit _spirit;
	readonly VirtualUser _user;

	public ToDreamAThousandDeaths_Tests(){
		_spirit = new Bringer();
		_user = new VirtualUser(_spirit);
		_board = Board.BuildBoardA();
		_gameState = new GameState( _spirit, _board );
		_gameState.Initialize();
		_ = _gameState.StartAction( ActionCategory.Spirit_Power ); // !!! get rid of or Dispose
		_ctx = MakeFreshPowerCtx();

		// Disable destroying presence
		_ctx.GameState.ModifyBlightAddedEffect.ForGame.Add( x => { x.Cascade = false; x.DestroyPresence = false; } );

	}

	TargetSpaceCtx MakeFreshPowerCtx() {
		var ctx = _spirit.BindMyPowers(_gameState ); // This is correct usage.
		return ctx.Target( _board[5] );
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
		_ctx.Tokens.AdjustDefault(Human.Explorer, count );

		// When: causing 1 damage to each invader
		switch(method) {
			case "damage": _ = OneDamageToEachAsync( _ctx ); break;
			case "destroy": _ = DestroyAllExplorersAndTownsAsync( _ctx ); break;
		}

		// Then: dream-death allows User pushes them
		for(int i = 0; i < count; ++i)
			_user.PushSelectedTokenTo( "E@1", "A1,A4,A6,[A7],A8" );

		// And: 0-fear
		Assert_GeneratedFear( 0 );

		//  and: explorer on destination
		_ctx.GameState.Assert_DreamingInvaders( _board[7], $"{count}E@1" );
		//  and: not at origin
		_ctx.GameState.Assert_Invaders( _board[5], $"" );
	}

	[Fact]
	public void KillingTown_GeneratesFear_PushesIt() {
		int count = 2;
		// generate 2 fear per town destroyed,
		// pushes town

		// Given: 2 town
		_ctx.Tokens.AdjustDefault( Human.Town, count );

		// When: destroying towns
		_ = DestroyAllExplorersAndTownsAsync( _ctx );

		// Then: dream-death allows User pushes them
		for(int i = 0; i < count; ++i)
			_user.PushSelectedTokenTo( "T@2","A1,A4,A6,[A7],A8" );

		// And:4-fear
		Assert_GeneratedFear( count * 2 );

		//  and: town on destination
		_ctx.GameState.Assert_DreamingInvaders( _board[7], $"{count}T@2" );
		//  and: not at origin
		_ctx.GameState.Assert_Invaders( _board[5], $"" );

	}

	[Fact]
	public void DreamDamageResetsEachPower() {

		// Given: 2 explorers
		_ctx.Tokens.AdjustDefault( Human.City, 1 );

		// When: 3 separate actinos cause 1 damage
		async Task Run3Async() {
			await Run_OneDamageToEachAsync();
			await Run_OneDamageToEachAsync();
			await Run_OneDamageToEachAsync();
		}
		_ = Run3Async();

		_user.Assert_Done();

		// And: 0-fear
		Assert_GeneratedFear( 0 ); // city never destroyed
	}

	async Task Run_OneDamageToEachAsync() {
		await using var actionScope = this._gameState.StartAction( ActionCategory.Spirit_Power );
		await OneDamageToEachAsync( MakeFreshPowerCtx() );
	}

	[Fact]
	public async Task ConsecutivePowersCanDreamKillMultipletimes() {

		// Given: 1 very-damaged city
		_ctx.Tokens.Adjust( StdTokens.City1, 1 );

		// When: 3 separate actinos cause 1 damage
		// EACH power gets a fresh ctx so INVADERS can reset
		await Run_OneDamageToEachAsync();
		await Run_OneDamageToEachAsync();
		await Run_OneDamageToEachAsync();
		_user.Assert_Done();

		// And: 0-fear
		Assert_GeneratedFear( 3*5 ); // city never destroyed
		// City still there
		_ctx.Tokens[ StdTokens.City1 ].ShouldBe(1);
	}

	[Fact]
	public void MaxKillOnce() {
		// Given: 1 very-damaged city
		_ctx.Tokens.Adjust( StdTokens.City1, 1 );

		// When: doing 4 points of damage
		async Task PlayCard() { try { await FourDamage( MakeFreshPowerCtx() ); } catch( Exception ex) {
			_ = ex.ToString();
		} }
		_ = PlayCard();

		_user.SelectsDamageRecipient(4,"C@1");

		// And: 0-fear
		Assert_GeneratedFear( 1 * 5 ); // city only destroyed once

		// Dreaming City with partial damage still there
		_ctx.Tokens[BringerSpaceCtx.ToggleDreamer(StdTokens.City1) ].ShouldBe( 1 );
	}

	void Assert_GeneratedFear( int expectedFearCount ) {
		int actualGeneratedFear = _ctx.GameState.Fear.EarnedFear
			+ 4 * _ctx.GameState.Fear.ActivatedCards.Count;
		actualGeneratedFear.ShouldBe(expectedFearCount,"fear countis wrong");
	}

}
