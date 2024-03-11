namespace SpiritIsland.Tests.Spirits.BringerNS;

[Collection("BaseGame Spirits")]
[Trait("SpecialRule","ToDreamAThousandDeaths")]
public class ToDreamAThousandDeaths_Tests {

	readonly Board _board;
	readonly GameState _gameState;
	readonly Spirit _spirit;

	public ToDreamAThousandDeaths_Tests(){
		_spirit = new Bringer();
		_board = Board.BuildBoardA();
		_gameState = new GameState( _spirit, _board );
		_gameState.Initialize();

		// Disable destroying presence
		_gameState.DisableBlightEffect();

	}

	TargetSpaceCtx MakeFreshPowerCtx(ActionScope actionScope) {
		actionScope.Owner = _spirit;
		return _spirit.Target( _board[5] );
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
	public async Task DreamKilledExplorers_ArePushed(string method) {
		const int count = 2;

		var tokens = _board[5].ScopeTokens;

		// Given: 2 explorers
		tokens.Given_InitSummary("2E@1");

		// When: damaging / destroying each invader
		await using ActionScope scope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,_spirit);
		Func<TargetSpaceCtx,Task> powerCardActionAsync = (method switch { "damage" => OneDamageToEachAsync, "destroy" => DestroyAllExplorersAndTownsAsync, _ => throw new Exception(nameof(method)) });
		await powerCardActionAsync( MakeFreshPowerCtx( scope ) )
			.AwaitUser( user => {
				// Then: dream-death allows User pushes them
				for(int i = 0; i < count; ++i)
					user.PushSelectedTokenTo( "E@1", "A1,A4,A6,[A7],A8" );
			} ).ShouldComplete(method);

		// And: 0-fear
		Assert_GeneratedFear( 0 );

		//  and: explorer on destination
		var gs = GameState.Current;
		_board[7].Assert_DreamingInvaders( $"{count}E@1" );
		//  and: not at origin
		_board[5].Assert_HasInvaders("");
	}

	[Fact]
	public async Task KillingTown_GeneratesFear_PushesIt() {
		const int count = 2;
		// generate 2 fear per town destroyed,
		// pushes town

		// Given: 2 towns on A5
		_board[5].Given_HasTokens($"{count}T@2");
		//   And: nothing on A8
		_board[8].Given_ClearTokens();

		// When: destroying towns
		await _spirit.When_TargetingSpace( _board[5], DestroyAllExplorersAndTownsAsync, (user)=>{
			// Then: dream-death allows User pushes them
			for(int i = 0; i < count; ++i)
				user.PushSelectedTokenTo( "T@2", "A1,A4,A6,A7,[A8]" );
		} );

		// And:4-fear
		Assert_GeneratedFear( count * 2 );
		// And: town on destination
		_board[8].ScopeTokens.Summary.ShouldBe( $"{count}T@2" );
		// And: not at origin
		_board[5].Assert_HasInvaders("");

	}

	[Fact]
	public async Task DreamDamageResetsEachPower() {

		// Given: 2 explorers
		_board[5].Given_HasTokens("1C@3");
		// ctx.Tokens.AdjustDefault( Human.City, 1 );

		// When: 3 separate actinos cause 1 damage
		async Task Run3Async() {
			await Run_OneDamageToEachAsync();
			await Run_OneDamageToEachAsync();
			await Run_OneDamageToEachAsync();
		}
		await Run3Async().ShouldComplete("run-3");

		// And: 0-fear
		Assert_GeneratedFear( 0 ); // city never destroyed
	}

	async Task Run_OneDamageToEachAsync() {
		await using var actionScope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,_spirit);
		await OneDamageToEachAsync( MakeFreshPowerCtx( actionScope ) );
	}

	[Fact]
	public async Task ConsecutivePowersCanDreamKillMultipletimes() {

		// Given: 1 very-damaged city
		var tokens = _board[5].ScopeTokens;
		tokens.Setup( StdTokens.City1, 1 );

		// When: 3 separate actinos cause 1 damage
		// EACH power gets a fresh ctx so INVADERS can reset
		await Run_OneDamageToEachAsync();
		await Run_OneDamageToEachAsync();
		await Run_OneDamageToEachAsync();

		// And: 0-fear
		Assert_GeneratedFear( 3*5 ); // city never destroyed
		// City still there
		tokens[ StdTokens.City1 ].ShouldBe(1);
	}

	[Fact]
	public async Task MaxKillOnce() {

		var log = GameState.Current.LogAsStringList();

		var tokens = _board[5].ScopeTokens;

		// Given: 1 very-damaged city
		tokens.Setup( StdTokens.City1, 1 );

		{
			await using ActionScope scope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,_spirit);

			// When: doing 4 points of damage
			await FourDamage( MakeFreshPowerCtx( scope ) )
				.AwaitUser( user=> {
					user.NextDecision.HasPrompt( "Damage (4 remaining)" ).HasOptions( "C@1" ).Choose( "C@1" );
				} ).ShouldComplete("4 damage");

			// Then: 0-fear
			Assert_GeneratedFear( 1 * 5 ); // city only destroyed once

			//  And: Dreaming City with partial damage still there
			tokens[TDaTD_ActionTokens.ToggleDreamer(StdTokens.City1) ].ShouldBe( 1 );
		}

		// But: after scope done, original is restored
		tokens[StdTokens.City1].ShouldBe( 1 );
	}

	static void Assert_GeneratedFear( int expectedFearCount ) {
		var fear = GameState.Current.Fear;
		int actualGeneratedFear = fear.EarnedFear + 4 * fear.ActivatedCards.Count;
		actualGeneratedFear.ShouldBe(expectedFearCount,"fear countis wrong");
	}

}
