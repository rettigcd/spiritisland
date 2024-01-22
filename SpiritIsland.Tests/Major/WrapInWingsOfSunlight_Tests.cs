namespace SpiritIsland.Tests;

public class WrapInWingsOfSunlight_Tests {

	[Trait("Feature","Move")]
	[Fact]
	public async Task Move5() {
		// Setup
		var spirit = new TestSpirit(PowerCard.For(typeof(WrapInWingsOfSunlight)));
		var user = new VirtualUser( spirit );
		var board = Board.BuildBoardA();
		var gameState = new GameState( spirit, board );
		gameState.DisableInvaderDeck();
		gameState.Initialize();

		// Given: A3 has 10 Dahans
		var src = board[3];
		gameState.Tokens[src].Dahan.Init(10);

		//  And: spirit has presence on A3
		spirit.Given_IsOn(src);

		//  And: Destination has no Dahan on it
		var dst = board[1];
		gameState.Tokens[dst].Dahan.Init(0);

		// When: playing Card 
		await spirit.When_ResolvingCard<WrapInWingsOfSunlight>((user)=> {
			user.Choose(src.Text);
			user.NextDecision.HasPrompt( "Move up to (5)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" )
				.HasDestinationOptions( "A1,A2,A3,A4,A5,A6,A7,A8" ).MoveTo( dst.Text );

			user.NextDecision.HasPrompt( "Move up to (4)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" ); // pick remaining tokens
			user.NextDecision.HasPrompt( "Move up to (3)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" );
			user.NextDecision.HasPrompt( "Move up to (2)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" );
			user.NextDecision.HasPrompt( "Move up to (1)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" );
		} );

		// Then: target 2 of each
		var dstTokens = gameState.Tokens[dst];
		dstTokens[StdTokens.Dahan].ShouldBe(5);
	}

	[Trait("Feature","Push")]
	[Fact]
	public async Task TerrifyingChase_PushDahan_NoBeast() {
		// Setup
		var spirit = new TestSpirit(PowerCard.For(typeof(TerrifyingChase)));
		var user = new VirtualUser( spirit );
		var board = Board.BuildBoardA();
		var gameState = new GameState( spirit, board );
		gameState.DisableInvaderDeck();
		gameState.Initialize();

		// Given: A5 has 3 Towns, Dahans, and Explorers
		var src = board[5];
		var tokens = gameState.Tokens[src];
		tokens.InitDefault( Human.Dahan , 3);
		tokens.InitDefault( Human.Explorer, 3);
		tokens.InitDefault( Human.Town    , 3);
		tokens.Beasts.Init(0);

		//  And: spirit has presence on A5
		spirit.Given_IsOn(src);

		//  And: dst has nothing on it
		var dst = board[8];

		// When: playing Card
		await spirit.When_ResolvingCard<TerrifyingChase>( (user) => {
			user.Choose(src.Text);
			//  And: bringing 2 of each
			user.NextDecision.HasPrompt("Push (2)")
				.HasSourceOptions("D@2,E@1,T@2").MoveFrom("D@2")
				.HasDestinationOptions("A1,A4,A6,A7,A8").MoveTo(dst.Label);
			user.NextDecision.HasPrompt("Push (1)")
				.HasSourceOptions("D@2,E@1,T@2").MoveFrom("D@2")
				.HasDestinationOptions("A1,A4,A6,A7,A8").MoveTo(dst.Label);
		} );

		// Then: target 2 of each
		var dstTokens = gameState.Tokens[dst];
		dstTokens[StdTokens.Dahan].ShouldBe(2);
	}

}