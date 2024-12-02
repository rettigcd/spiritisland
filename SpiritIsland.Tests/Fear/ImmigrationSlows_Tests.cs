namespace SpiritIsland.Tests.Fear; 

public sealed class ImmigrationSlows_Tests {

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task Level1_SkipBuildInLowestNumberedLand() {

		// Setup:
		var gs = new SoloGameState();
		await gs.InvaderDeck.InitExploreSlotAsync();
		await gs.InvaderDeck.AdvanceAsync();

		// Given 1 build card
		var card = gs.InvaderDeck.Build.Cards.Single();

		//   And: 2 matching spaces
		var matchingSpaces = gs.Spaces_Unfiltered.Where(card.MatchesCard).ToArray();
		matchingSpaces.Length.ShouldBe(2);
		var toSkip = matchingSpaces[0];
		var toBuild = matchingSpaces[1];

		// Given: explorer on both
		toSkip.Given_InitSummary("1E@1");
		toBuild.Given_InitSummary("1E@1");

		//  When: Level 1 activated - During the next normal build, skip the lowest-numbered land matching the Invader card on each board.";
		await new ImmigrationSlows().ActAsync(1);

		//   And: Build Slot Activated
		await gs.InvaderDeck.Build.Execute(gs);

		//  Then: lowest # land is skipped
		toSkip.Summary.ShouldBe("1E@1");
		//   And: other was built on 
		toBuild.Summary.ShouldBe("1E@1,1T@2");
	}

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async Task Level2_DelayBuild1Round() {

		var gs = new SoloGameState();
		await gs.InvaderDeck.InitExploreSlotAsync();
		await gs.InvaderDeck.AdvanceAsync();

		// Given: a card in the build deck
		var buildCard = gs.InvaderDeck.Build.Cards.Single();

		//  And: exactly 1 explorer on each matching space
		var matches = gs.Spaces_Unfiltered.Where(buildCard.MatchesCard).ToList();
		foreach( var match in matches )
			match.Given_InitSummary("1E@1");

		// When: Do Level-2: Skip the next normal Build. The Build card remains in place instead of shifting left.
		await new ImmigrationSlows().ActAsync(2);

		//  And: doing Invader phase
		await gs.InvaderDeck.AdvanceAsync();

		// Then: Neither Space was built on
		foreach( var match in matches )
			match.Summary.ShouldBe("1E@1");

		//  And: Build deck still has original
		gs.InvaderDeck.Build.Cards.Contains(buildCard).ShouldBeTrue();
		//  And: 1 more
		gs.InvaderDeck.Build.Cards.Count.ShouldBe(2);
	}

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Fact]
	public async void Level3_SkipBuild1Round() {

		var gs = new SoloGameState();
		gs.Initialize();
		var invaders = gs.InvaderDeck;

		// Given: a card in the build deck
		await invaders.AdvanceAsync();
		var buildCard = invaders.Build.Cards.Single();

		//  And: exactly 1 explorer on each matching space
		var matches = gs.Spaces_Unfiltered.Where(buildCard.MatchesCard).ToList();
		foreach( var match in matches )
			match.Given_InitSummary("1E@1");

		// When: Do level-3: "Skip the next normal Build. The Build card shifts left as usual."
		await new ImmigrationSlows().ActAsync(3);

		//  And: doing Invader phase
		await invaders.AdvanceAsync();

		// Then: Neither Space was built on
		foreach( var match in matches )
			match.Summary.ShouldBe("1E@1");

		//  And: Card moved to Ravage deck
		invaders.Ravage.Cards.Contains(buildCard).ShouldBeTrue();

		//  And: 1 new card in explore slot
		invaders.Build.Cards.Single().ShouldNotBe(buildCard);

	}

}

