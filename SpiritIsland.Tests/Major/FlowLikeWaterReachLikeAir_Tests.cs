namespace SpiritIsland.Tests.Major;

public class FlowLikeWaterReachLikeAir_Tests {

	[Trait("Feature","Push")]
	[Fact]
	public async Task CanBring_2TownDahanExplorer() {
		// Setup
		TestSpirit spirit = new TestSpirit(PowerCard.ForDecorated(FlowLikeWaterReachLikeAir.ActAsync));
		Board board = Boards.A;
		GameState gameState = new SoloGameState( spirit, board );

		// Given: A5 has 3 Towns, Dahans, Explorers, & Presence(TS)
		SpaceSpec a5 = board[5];
		Space space = gameState.Tokens[a5];
		space.Given_HasTokens("3D@2,3E@1,3T@2,1TS");
		//  And: A1 has nothing on it
		SpaceSpec a1 = board[1];

		// When: playing Card
		await spirit.When_ResolvingCard<FlowLikeWaterReachLikeAir>( (user) => {
			//  And: Can bring 2 of each
			// !!! this should be implemented as a Push
			user.NextDecision .HasPrompt( "Select Presence to push." ).HasOptions( "TS,Done" ).Choose( "TS" );
			user.AssertDecisionInfo( "Push Presence to", "[A1],A4,A6,A7,A8" );

			user.NextDecision.HasPrompt( "Bring up to (6)" ).MoveFrom("D@2","D@2,E@1,T@2,Done");
			user.NextDecision.HasPrompt( "Bring up to (5)" ).MoveFrom("D@2","D@2,E@1,T@2,Done");
			user.NextDecision.HasPrompt( "Bring up to (4)" ).MoveFrom("E@1","E@1,T@2,Done");
			user.NextDecision.HasPrompt( "Bring up to (3)" ).MoveFrom("E@1","E@1,T@2,Done");
			user.NextDecision.HasPrompt( "Bring up to (2)" ).MoveFrom("T@2","T@2,Done");
			user.NextDecision.HasPrompt( "Bring up to (1)" ).MoveFrom("T@2","T@2,Done");
		} );

		// Then: target 2 of each
		a1.ScopeSpace.Summary.ShouldBe( "2D@2,2E@1,2T@2,1TS" );
	}

	[Trait( "Targeting", "Range" )]
	[Fact]
	public async Task ExtendsRange2() {
		// Setup
		TestSpirit spirit = new TestSpirit( PowerCard.ForDecorated(FlowLikeWaterReachLikeAir.ActAsync) );
		Board board = Boards.A;
		GameState gameState = new SoloGameState( spirit, board );

		// Given: Presence on A8 Presence(TS)
		board[3].ScopeSpace.Given_HasTokens( "1TS" );
		//   And: played Flow Like Water
		await spirit.When_ResolvingCard<FlowLikeWaterReachLikeAir>( u => u.Choose( "Done" ) );

		//  When: targetting a power of Range 0
		await spirit.When_ResolvingCard<MesmerizedTranquility>( user => {
			// Then: everything in range 2 is available
			user.NextDecision.HasOptions("A1,A2,A3,A4,A5").Choose("A5");
		} );
	}

}


