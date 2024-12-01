namespace SpiritIsland.Tests.Fear;

public sealed class ExplorersAreReluctant_Tests {

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task NormalInvaderPhases() {

		var spirit = new TestSpirit( PowerCard.For(typeof(CallToTend)) );

		GameState gs = new GameState( spirit, Board.BuildBoardA() ) {
			InvaderDeck = InvaderDeckBuilder.Default.Build() // Same order every time
		};
		gs.Initialize(); 

		// Disable destroying presence
		GameState.Current.DisableBlightEffect();
		var log = GameState.Current.LogInvaderActions();
		log.Clear(); // clear out initial Explorer setup

		await InvaderPhase.ActAsync( gs );
		log.Assert_Built( "A3", "A8" );
		log.Assert_Explored( "A2", "A5" );

		await InvaderPhase.ActAsync( gs );
		log.Assert_Ravaged( "A3", "A8" );
		log.Assert_Built( "A2", "A5" );
		log.Assert_Explored( "A4", "A7" );

		await InvaderPhase.ActAsync( gs );
		log.Assert_Ravaged( "A2", "A5" );
		log.Assert_Built( "A4", "A7" );
		log.Assert_Explored( "A3", "A8" );

	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task Level1_SkipExploreInLowestNumberedLand() {

		// Setup:
		var gs = new GameState(new ShiftingMemoryOfAges(), Boards.A);
		await gs.InvaderDeck.InitExploreSlotAsync();
		var card = gs.InvaderDeck.Explore.Cards.First();
		var matchingSpaces = gs.Spaces_Unfiltered.Where(card.MatchesCard).ToList();
		var toSkip = matchingSpaces[0];
		var toExplore = matchingSpaces[1];

		// Given: town on both
		toSkip.Given_InitSummary("1T@2");
		toExplore.Given_InitSummary("1T@2");

		//  When: Level 1 activated - "During the next normal explore, skip the lowest-numbered land matching the invader card on each board."
		await new ExplorersAreReluctant().ActAsync(1);

		//   And: Explore Card Activated
		await gs.InvaderDeck.Explore.Execute(gs);

		//  Then: lowest # land is skipped
		toSkip.Summary.ShouldBe("1T@2");
		//   And: other was explored
		toExplore.Summary.ShouldBe("1E@1,1T@2");
	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task Level2_DelayExplore1Round() {

		var gs = new GameState(new ShiftingMemoryOfAges(), Boards.B );
		gs.Initialize();

		// Given: a card in the explore deck
		var exploreCard = gs.InvaderDeck.Explore.Cards.Single();

		//  And: exactly 1 town on each matching space
		var matches = gs.Spaces_Unfiltered.Where(exploreCard.MatchesCard).ToList();
		foreach(var match in matches)
			match.Given_InitSummary("1T@2");

		// When: Do Level-2: Skip the next normal explore.  During the next invader phase, draw an adidtional explore card.
		await new ExplorersAreReluctant().ActAsync(2);

		//  And: doing Invader phase
		await gs.InvaderDeck.AdvanceAsync();

		// Then: Neither Space was explored
		foreach( var match in matches )
			match.Summary.ShouldBe("1T@2");

		//  And: Explore deck still has original
		gs.InvaderDeck.Explore.Cards.Contains(exploreCard).ShouldBeTrue();
		//  And: 1 more
		gs.InvaderDeck.Explore.Cards.Count.ShouldBe(2);

	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task Level3_SkipExplore1Round() {

		var gs = new GameState(new ShiftingMemoryOfAges(), Boards.B);
		gs.Initialize();

		// Given: a card in the explore deck
		var exploreCard = gs.InvaderDeck.Explore.Cards.Single();

		//  And: exactly 1 town on each matching space
		var matches = gs.Spaces_Unfiltered.Where(exploreCard.MatchesCard).ToList();
		foreach( var match in matches )
			match.Given_InitSummary("1T@2");

		// When: Do level-3: Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual.
		await new ExplorersAreReluctant().ActAsync(3);

		//  And: doing Invader phase
		await gs.InvaderDeck.AdvanceAsync();

		// Test!!!: And do the flag (escalation) if relavent...

		// Then: Neither Space was explored
		foreach( var match in matches )
			match.Summary.ShouldBe("1T@2");

		//  And: Card moved to Build deck
		gs.InvaderDeck.Build.Cards.Contains(exploreCard).ShouldBeTrue();

		//  And: 1 new card in explore slot
		gs.InvaderDeck.Explore.Cards.Single().ShouldNotBe(exploreCard);

	}

}