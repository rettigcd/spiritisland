

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
		spirit.Presence.PlaceOn(src,gameState);

		//  And: Destination has no Dahan on it
		var dst = board[1];
		gameState.Tokens[dst].Dahan.Init(0);

		// When: playing Card
		static async Task PlayCard(TargetSpaceCtx ctx) { try { await WrapInWingsOfSunlight.ActAsync( ctx ); } catch(Exception ex) { 
			_ = ex.ToString(); 
		} } 
		_ = PlayCard( spirit.BindMyPower( gameState ).Target(src) );

		//  And: Can bring 2 of each
		user.AssertDecision( "Push Dahan to", "A1,A2,A3,A4,A5,A6,A7,A8,Done", dst.Label );
		user.AssertDecisionX( "Move up to (5)", "(D@2),Done" );
		user.AssertDecisionX( "Move up to (4)", "(D@2),Done" );
		user.AssertDecisionX( "Move up to (3)", "(D@2),Done" );
		user.AssertDecisionX( "Move up to (2)", "(D@2),Done" );
		user.AssertDecisionX( "Move up to (1)", "(D@2),Done" );

		// Then: target 2 of each
		var dstTokens = gameState.Tokens[dst];
		dstTokens[Tokens.Dahan].ShouldBe(5);
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
		tokens.InitDefault( TokenType.Dahan , 3);
		tokens.InitDefault( Invader.Explorer, 3);
		tokens.InitDefault( Invader.Town    , 3);

		//  And: spirit has presence on A5
		spirit.Presence.PlaceOn(src,gameState);

		//  And: dst has nothing on it
		var dst = board[8];

		// When: playing Card
		static async Task PlayCard(TargetSpaceCtx ctx) { try { await TerrifyingChase.ActAsync( ctx ); } catch(Exception ex) { 
			_ = ex.ToString(); 
		} } 
		_ = PlayCard( spirit.BindMyPower( gameState ).Target(src) );

		//  And: Can bring 2 of each
		user.AssertDecisionX( "Push (2)", "(D@2),E@1,T@2" );
		user.AssertDecision( "Push D@2 to", "A1,A4,A6,A7,A8", dst.Label );
		user.AssertDecisionX( "Push (1)", "(D@2),E@1,T@2" );
		user.AssertDecision( "Push D@2 to", "A1,A4,A6,A7,A8", dst.Label );

		// Then: target 2 of each
		var dstTokens = gameState.Tokens[dst];
		dstTokens[Tokens.Dahan].ShouldBe(2);
	}

}