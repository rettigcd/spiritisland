namespace SpiritIsland.Tests.Spirits.ManyMinds;

public class ManyMinds_Tests {

	public ManyMinds_Tests(){
		ActionScope.Initialize();
	}

	[Fact]
	[Trait( "Feature", "Gather" )]
	public async Task GrowthGathering_HasRange2() {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Board.BuildBoardA();
		GameState gs = new GameState(spirit,board);

		// Given: presence on A1
		spirit.Given_HasPresenceOn( board[1] );

		//   And: Beast on 7
		board[7].Given_HasTokens("1A");

		await spirit.When_ResolvingCard<PreyOnTheBuilders>((user)=> {
			user.Choose( "A1" );

			// Then: can gather beast 2 spaces away
			user.Choose( "Beast on A7" );
		} );
	}

	[Fact]
	[Trait("Feature","Gather")]
	public async Task PowerGathering_HasRange2() {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board );

		// Given: presence on A3
		spirit.Given_HasPresenceOn( board[4] );

		//   And: Beast on 7
		board[7].Given_HasTokens( "1A" );

		await spirit.When_Growing( () => {
			VirtualUser u = new VirtualUser(spirit);
			// Then: can gather 1 beast
			u.NextDecision.HasPrompt( "Select Growth" ).Choose( "Gather1Token" );

			// Implemented as a Push
			u.NextDecision.HasPrompt( "Select space to Gather a Beast" ).Choose( "A1" );
			u.NextDecision.HasPrompt( "Gather up to 1 Beast" ).Choose( "Beast on A7" );

			// Implemented as a Gather
			// spirit.NextDecision().HasPrompt("").Choose( "A1" );
			// spirit.NextDecision().Choose( "Beast on A7" );

			// Cleanup
			u.NextDecision.Choose( "PlacePresenceAndBeast" );
			u.NextDecision.HasPrompt( "Select Presence to place" ).Choose( "1 energy" );
			u.NextDecision.Choose( "A1" );
		} );

	}

	// when moving MM-Beast, presence seems to be duplicating or incrementing in destination
	[Theory]
	[InlineData( 2, 0, false )]
	[InlineData( 3, 0, false )]
	[InlineData( 4, 0, true )]
	[InlineData( 2, 2, false )]
	[Trait("Feature","Move")]
	public async Task MovingMMBeast_Moves2Presence(int startingSrcPresence, int startingDstPresence, bool srcEndsWithBeast) {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Board.BuildBoardA();
		_ = new GameState( spirit, board );
		SpaceState srcTokens = board[1].Tokens;
		SpaceState dstTokens = board[2].Tokens;

		// Given: presence on A1 (src)
		await srcTokens.Add(spirit.Presence.Token, startingSrcPresence ).ShouldComplete("add 2 presence"); // use .Add event triggers extra beast

		//   And: maybe presence on A2 (dest)
		if(0<startingDstPresence)
			await dstTokens.Add( spirit.Presence.Token, startingSrcPresence ).ShouldComplete( "add 2 presence" ); // use .Add event triggers extra beast

		//  When: moving
		IToken beast = (IToken)srcTokens.OfClass(Token.Beast).First();
		await srcTokens.MoveTo(beast, dstTokens ).ShouldComplete("move best");

		// Then
		spirit.Presence.CountOn(srcTokens).ShouldBe( startingSrcPresence-2 );
		spirit.Presence.CountOn(dstTokens).ShouldBe( startingDstPresence+2 );
		srcTokens[beast].ShouldBe(srcEndsWithBeast ? 1 :0 );
		dstTokens[beast].ShouldBe( 1 );

	}


	[Fact]
	[Trait( "Feature", "Move" )]
	public async Task GuideTheWay_CantSwitchBeastsMidflight() {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board );

		// Given: 2 presence and 2 dahan on A1
		spirit.Given_HasPresenceOn( board[1],2 );
		await board[1].Tokens.Add( spirit.Presence.Token, 2 ).ShouldComplete( "add 2 presence" ); // use .Add event triggers extra beast

		board[1].Given_HasTokens("2D@2");
		//   And: Beast on 5
		board[5].Given_HasTokens( "1A" );

		// When: playing Guide the Way on Feathered Wings
		await spirit.When_ResolvingCard<GuideTheWayOnFeatheredWings>( (user) => {
			user.Choose( "A1" );
			user.NextDecision.HasPrompt( "Move up to (1)" ).Choose( "SS-Beast" );
			user.NextDecision.HasPrompt( "Move token to" ).Choose( "A5" );
			user.NextDecision.HasPrompt( "Move up to 2 Dahan" ).Choose( "D@2 on A1" );
			user.NextDecision.HasPrompt( "Move up to 1 Dahan" ).Choose( "D@2 on A1" );

			// Then: only original SS-Beast is available for 2nd step. (the other Beast token is not an option)
			user.NextDecision.HasPrompt( "Move SS-Beast to" ).Choose( "A7" );

			// Cleanup
			user.NextDecision.HasPrompt( "Move up to 2 Dahan" ).Choose( "D@2 on A5" );
			user.NextDecision.HasPrompt( "Move up to 1 Dahan" ).Choose( "D@2 on A5" );
		} );

		//  And: Final slot has 2 presence, 2 dahan, and 1 beast
		board[7].Tokens.Summary.ShouldBe("2D@2,2MMMaO,1SS-Beast");
	}

	[Fact]
	public async Task BesetAndCounfound_Requires2BeastAndInvaders() {
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

		await spirit.When_ResolvingInnate<BesetAndConfoundTheInvaders>( (user) => {
			// Then: only the space with both is targetable
			user.NextDecision.HasPrompt( "Beset and Confound the Invaders: Target Space" ).HasOptions("A7").Choose("A7");
		} );

		// And: should have defend-2
		board[7].Tokens.Summary.ShouldBe("2A,2E@1,2G");
	}
}
