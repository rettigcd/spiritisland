namespace SpiritIsland.Tests.Major;

public class IndomitableClaim_Tests {

	// ! should also test without meeting element threshold

	[Trait( "Invaders", "Ravage" )]
	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Build" )]
	[Fact]
	public async Task Threshold_StopsAllInvaderActions() {

		var gs = new SoloGameState();
		var space = gs.Board[2].ScopeSpace;

		// Given: a host of Dahan and Invaders
		space.Given_InitSummary("1C@3,2D@2,5E@1,2T@2");

		//   And: spirit has some elements
		gs.Spirit.Configure().Elements("2 sun,3 earth");

		// When: card played
		await IndomitableClaim.ActAsync(gs.Spirit.Target(space)).AwaitUser(user => {
			user.NextDecision.HasPrompt("Select Presence to place").ChooseFirst();
			user.NextDecision.HasPrompt("Activate Element Threshold?").Choose("Yes");
		}).ShouldComplete(ParalyzingFright.Name);

		//  And: Invaders: Ravage, Build, Explore
		var card = space.BuildInvaderCard();
		await card.When_Ravaging();
		await card.When_Building();
		await card.When_Exploring();

		// Then: none of that happened.
		space.Summary.ShouldBe("1C@3,2D@2,5E@1,20G,2T@2,1TS");

		//  And: 4 fear were generated
		int actualFear = gs.Fear.ActivatedCards.Count * 4 + gs.Fear.EarnedFear;
		actualFear.ShouldBe(3);

	}

}
