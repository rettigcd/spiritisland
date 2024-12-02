namespace SpiritIsland.Tests.Spirits.ManyMinds;

public class ManyMinds_Tests {

	public ManyMinds_Tests(){}

	[Fact]
	[Trait( "Feature", "Gather" )]
	public async Task GrowthGathering_HasRange2() {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Boards.A;
		GameState gs = new SoloGameState(spirit,board);

		// Given: presence on A1
		spirit.Given_IsOn( board[1] );

		//   And: Beast on 7
		board[7].Given_HasTokens("1A");

		await spirit.When_ResolvingCard<PreyOnTheBuilders>((user)=> {
			user.Choose( "A1" );

			// Then: can gather beast 2 spaces away
			user.NextDecision.MoveFrom( "Beast" );
		} );
	}

	[Fact]
	[Trait("Feature","Gather")]
	public async Task PowerGathering_HasRange2() {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Boards.A;
		GameState gs = new SoloGameState( spirit, board );

		// Given: presence on A3
		spirit.Given_IsOn( board[4] );

		//   And: Beast on 7
		board[7].Given_HasTokens( "1A" );

		await spirit.When_Growing( (user) => {
			// Then: can gather 1 beast
			user.NextDecision.HasPrompt( "Select Growth" ).Choose( "Gather 1 Token" );

			// Implemented as a Push
			user.NextDecision.HasPrompt( "Select space to Gather a Beast" ).Choose( "A1" );
			user.NextDecision.HasPrompt( "Gather up to (1)" ).MoveFrom( "Beast" );

			// Implemented as a Gather
			// spirit.NextDecision().HasPrompt("").Choose( "A1" );
			// spirit.NextDecision().Choose( "Beast on A7" );

			// Cleanup
			user.NextDecision.Choose( "Place Presence and Beast" );
			user.NextDecision.HasPrompt( "Select Presence to place" ).Choose( "1 energy" );
			user.NextDecision.Choose( "A1" );
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
		Board board = Boards.A;
		_ = new SoloGameState( spirit, board );
		Space src = board[1].ScopeSpace;
		Space dst = board[2].ScopeSpace;

		// Given: presence on A1 (src)
		await src.AddAsync(spirit.Presence.Token, startingSrcPresence ).ShouldComplete("add 2 presence"); // use .Add event triggers extra beast

		//   And: maybe presence on A2 (dest)
		if(0<startingDstPresence)
			await dst.AddAsync( spirit.Presence.Token, startingSrcPresence ).ShouldComplete( "add 2 presence" ); // use .Add event triggers extra beast

		//  When: moving
		IToken beast = src.OfTag(Token.Beast).First();
		await beast.MoveAsync( src,dst ).ShouldComplete("move best");

		// Then
		spirit.Presence.CountOn(src).ShouldBe( startingSrcPresence-2 );
		spirit.Presence.CountOn(dst).ShouldBe( startingDstPresence+2 );
		src[beast].ShouldBe(srcEndsWithBeast ? 1 :0 );
		dst[beast].ShouldBe( 1 );

	}


	[Fact]
	[Trait( "Feature", "Move" )]
	public async Task GuideTheWay_CantSwitchBeastsMidflight() {
		ManyMindsMoveAsOne spirit = new ManyMindsMoveAsOne();
		Board board = Boards.A;
		GameState gs = new SoloGameState( spirit, board );

		// Given: 2 presence and 2 dahan on A1
		spirit.Given_IsOn( board[1],2 );
		await board[1].ScopeSpace.AddAsync( spirit.Presence.Token, 2 ).ShouldComplete( "add 2 presence" ); // use .Add event triggers extra beast

		board[1].Given_HasTokens("2D@2");
		//   And: Beast on 5
		board[5].Given_HasTokens( "1A" );

		// When: playing Guide the Way on Feathered Wings
		await spirit.When_ResolvingCard<GuideTheWayOnFeatheredWings>( (user) => {
			user.Choose( "A1" );
			user.NextDecision.HasPrompt( "Move up to (1)" ).MoveFrom( "SS-Beast" ).MoveTo("A5");
			//user.NextDecision.HasPrompt( "Move SS-Beast to" ).Choose( "A5" );
			user.NextDecision.HasPrompt( "Move up to (2)" ).MoveFrom( "D@2" );
			user.NextDecision.HasPrompt( "Move up to (1)" ).MoveFrom( "D@2" );

			// Then: only original SS-Beast is available for 2nd step. (the other Beast token is not an option)
			user.NextDecision.HasPrompt( "Move SS-Beast to" ).Choose( "A7" );

			// Cleanup
			user.NextDecision.HasPrompt( "Move up to (2)" ).MoveFrom( "D@2" );
			user.NextDecision.HasPrompt( "Move up to (1)" ).MoveFrom( "D@2" );
		} );

		//  And: Final slot has 2 presence, 2 dahan, and 1 beast
		board[7].ScopeSpace.Summary.ShouldBe("2D@2,2MMMaO,1SS-Beast");
	}

	[Fact]
	public async Task BesetAndCounfound_Requires2BeastAndInvaders() {
		// Card reads that we need only target Invaders
		// But it is super confusing to target a space with 0 or 1 beasts
		// So I modified it to require 2 beasts also.
		// We don't want anyone accidentally putting it back

		Spirit spirit = new ManyMindsMoveAsOne();
		Board board = Boards.A;
		GameState gs = new SoloGameState(spirit, board);

		// Given: spirit on A5
		spirit.Given_IsOn( board[5].ScopeSpace );
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
			user.NextDecision.HasPrompt( "Beset and Confound the Invaders: Target Space" ).HasOptions("A1,A7").Choose("A7");
		} );

		// And: should have defend-2
		board[7].ScopeSpace.Summary.ShouldBe("2A,2E@1,2G");
	}
}
