namespace SpiritIsland.Tests;

public class WrapInWingsOfSunlight_Tests {

	[Trait("Feature","Move")]
	[Fact]
	public void Move5() {
		// Setup
		var spirit = new TestSpirit(PowerCard.For<WrapInWingsOfSunlight>());
		var user = new VirtualUser( spirit );
		var board = Board.BuildBoardA();
		var gameState = new GameState( spirit, board );
		gameState.DisableInvaderDeck();
		gameState.Initialize();

		// Given: A3 has 10 Dahans
		var src = board[3];
		gameState.Tokens[src].Dahan.Init(10);

		//  And: spirit has presence on A3
		spirit.Presence.PlaceOn(src, gameState).Wait();

		//  And: Destination has no Dahan on it
		var dst = board[1];
		gameState.Tokens[dst].Dahan.Init(0);

		// When: playing Card
		static async Task PlayCard(TargetSpaceCtx ctx) { try { await WrapInWingsOfSunlight.ActAsync( ctx ); } catch(Exception ex) { 
			_ = ex.ToString(); 
		} } 
		_ = gameState.StartAction( ActionCategory.Spirit_Power ); // !!! dispose
		_ = PlayCard( spirit.BindMyPowers().Target(src) );

		user.NextDecision.HasPrompt( "Move up to (5)" ).HasOptions("D@2,Done").Choose( "D@2" ); // Pick space and 1st token
		user.NextDecision.HasPrompt( "Move tokens to" ).HasOptions( "A1,A2,A3,A4,A5,A6,A7,A8" ).Choose( dst ); // pick destination
		user.NextDecision.HasPrompt( "Move up to (4) to A1" ).HasOptions("D@2,Done").Choose( "D@2" ); // pick remaining tokens
		user.NextDecision.HasPrompt( "Move up to (3) to A1" ).HasOptions("D@2,Done").Choose( "D@2" );
		user.NextDecision.HasPrompt( "Move up to (2) to A1" ).HasOptions("D@2,Done").Choose( "D@2" );
		user.NextDecision.HasPrompt( "Move up to (1) to A1" ).HasOptions("D@2,Done").Choose( "D@2" );

		// Then: target 2 of each
		var dstTokens = gameState.Tokens[dst];
		dstTokens[StdTokens.Dahan].ShouldBe(5);
	}

	[Trait("Feature","Push")]
	[Fact]
	public void TerrifyingChase_PushDahan_NoBeast() {
		// Setup
		var spirit = new TestSpirit(PowerCard.For<TerrifyingChase>());
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
		spirit.Presence.PlaceOn(src,gameState).Wait();

		//  And: dst has nothing on it
		var dst = board[8];

		// When: playing Card
		static async Task PlayCard(TargetSpaceCtx ctx) { try { await TerrifyingChase.ActAsync( ctx ); } catch(Exception ex) { 
			_ = ex.ToString(); 
		} } 
		_ = gameState.StartAction( ActionCategory.Spirit_Power ); // !!! Get rid of or Dispose
		_ = PlayCard( spirit.BindMyPowers().Target(src) );

		//  And: Can bring 2 of each
		user.AssertDecisionInfo( "Push (2)", "[D@2],E@1,T@2" );
		user.AssertDecision( "Push D@2 to", "A1,A4,A6,A7,A8", dst.Label );
		user.AssertDecisionInfo( "Push (1)", "[D@2],E@1,T@2" );
		user.AssertDecision( "Push D@2 to", "A1,A4,A6,A7,A8", dst.Label );

		// Then: target 2 of each
		var dstTokens = gameState.Tokens[dst];
		dstTokens[StdTokens.Dahan].ShouldBe(2);
	}

}