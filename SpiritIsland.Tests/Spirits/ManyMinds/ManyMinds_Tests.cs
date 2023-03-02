namespace SpiritIsland.Tests.Spirits.ManyMinds;

public class ManyMinds_Tests {

	public ManyMinds_Tests(){
		ActionScope.Initialize();
	}

	[Fact]
	[Trait( "Feature", "Gather" )]
	public void GrowthGathering_HasRange2() {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Board.BuildBoardA();
		GameState gs = new GameState(spirit,board);

		// Given: presence on A1
		spirit.Given_HasPresenceOn( board[1] );

		//   And: Beast on 7
		board[7].Given_HasTokens("1A");

		spirit.When_ResolvingCard<PreyOnTheBuilders>(()=> {
			spirit.NextDecision().Choose( "A1" );

			// Then: can gather beast 2 spaces away
			spirit.NextDecision().Choose( "Beast on A7" );
		} );
	}

	[Fact]
	[Trait("Feature","Gather")]
	public void PowerGathering_HasRange2() {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board );

		// Given: presence on A3
		spirit.Given_HasPresenceOn( board[3] );

		//   And: Beast on 7
		board[7].Given_HasTokens( "1A" );


		spirit.When_Growing( () => {
			// Then: can gather 1 beast
			spirit.NextDecision().Choose( "Gather1Beast" );
			// And: Target 2 spaces away (A1)
			spirit.NextDecision().Choose( "A1" );
			// Gather 2 spaces from that
			spirit.NextDecision().Choose( "Beast on A7" );

			// Cleanup
			spirit.NextDecision().Choose( "PlacePresenceAndBeast" );
			spirit.NextDecision().HasPrompt( "Select Presence to place" ).Choose( "1 energy" );
			spirit.NextDecision().Choose( "A1" );
		} );

	}

	// when moving MM-Beast, presence seems to be duplicating or incrementing in destination
	[Theory]
	[InlineData( 2, 0, false )]
	[InlineData( 3, 0, false )]
	[InlineData( 4, 0, true )]
	[InlineData( 2, 2, false )]
	[Trait("Feature","Move")]
	public void MovingMMBeast_Moves2Presence(int startingSrcPresence, int startingDstPresence, bool srcEndsWithBeast) {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board );
		SpaceState srcTokens = board[1].Tokens;
		SpaceState dstTokens = board[2].Tokens;

		// Given: presence on A1 (src)
		srcTokens.Add(spirit.Token, startingSrcPresence ).FinishUp("add 2 presence"); // use .Add event triggers extra beast

		//   And: maybe presence on A2 (dest)
		if(0<startingDstPresence)
			dstTokens.Add( spirit.Token, startingSrcPresence ).FinishUp( "add 2 presence" ); // use .Add event triggers extra beast

		//  When: moving
		IToken beast = (IToken)srcTokens.OfClass(Token.Beast).First();
		srcTokens.MoveTo(beast, dstTokens ).FinishUp("move best");

		// Then
		spirit.Presence.CountOn(srcTokens).ShouldBe( startingSrcPresence-2 );
		spirit.Presence.CountOn(dstTokens).ShouldBe( startingDstPresence+2 );
		srcTokens[beast].ShouldBe(srcEndsWithBeast ? 1 :0 );
		dstTokens[beast].ShouldBe( 1 );

	}


	[Fact]
	[Trait( "Feature", "Move" )]
	public void GuideTheWay_CantSwitchBeastsMidflight() {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board );

		// Given: 2 presence and 2 dahan on A1
		spirit.Given_HasPresenceOn( board[1],2 );
		board[1].Tokens.Add( spirit.Token, 2 ).FinishUp( "add 2 presence" ); // use .Add event triggers extra beast

		board[1].Given_HasTokens("2D@2");
		//   And: Beast on 5
		board[5].Given_HasTokens( "1A" );

		// When: playing Guide the Way on Feathered Wings
		spirit.When_ResolvingCard<GuideTheWayOnFeatheredWings>( () => {
			spirit.NextDecision().Choose( "A1" );
			spirit.NextDecision().HasPrompt( "Move up to (1)" ).Choose( "SS-Beast" );
			spirit.NextDecision().HasPrompt( "Move token to" ).Choose( "A5" );
			spirit.NextDecision().HasPrompt( "Move up to 2 Dahan" ).Choose( "D@2 on A1" );
			spirit.NextDecision().HasPrompt( "Move up to 1 Dahan" ).Choose( "D@2 on A1" );

			// Then: only original SS-Beast is available for 2nd step. (the other Beast token is not an option)
			spirit.NextDecision().HasPrompt( "Move SS-Beast to" ).Choose( "A7" );

			// Cleanup
			spirit.NextDecision().HasPrompt( "Move up to 2 Dahan" ).Choose( "D@2 on A5" );
			spirit.NextDecision().HasPrompt( "Move up to 1 Dahan" ).Choose( "D@2 on A5" );
		} );

		//  And: Final slot has 2 presence, 2 dahan, and 1 beast
		board[7].Tokens.Summary.ShouldBe("2D@2,2MMMaO,1SS-Beast");
	}

	[Fact]
	public void BesetAndCounfound_Requires2BeastAndInvaders() {
		// Card reads that we need only target Invaders
		// But it is super confusing to target a space with 0 or 1 beasts
		// So I modified it to require 2 beasts also.
		// We don't want anyone accidentally putting it back

		Spirit spirit = new ManyMindsMoveAsOne();
		Board board = Board.BuildBoardA();
		GameState gs = new GameState(spirit, board);

		// Given: spirit on A5
		spirit.Presence.Given_Adjust( board[5].Tokens, 1 );
		//   And: trigger elements
		spirit.Configure().Elements("1 air,2 animal");

		//   And: A1: Invaders but no beast
		board[1].Given_HasTokens("4E@1,2T@2");
		//   And: A4: 2 Beasts but no Invaders
		board[4].Given_HasTokens("2A");
		//   And: A7: 2 Beasts AND Invaders
		board[7].Given_HasTokens( "2A,2E@1" );

		spirit.When_ResolvingInnate<BesetAndConfoundTheInvaders>( () => {
			// Then: only the space with both is targetable
			spirit.NextDecision().HasPrompt( "Beset and Confound the Invaders: Target Space" ).HasOptions("A7").Choose("A7");
		} );

		// And: should have defend-2
		board[7].Tokens.Summary.ShouldBe("2A,2E@1,2G");
	}
}
