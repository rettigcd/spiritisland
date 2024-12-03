using SpiritIsland.Log;

namespace SpiritIsland.Tests.Major;

[Trait( "Invaders", "Ravage" )]
[Trait( "Invaders", "Build" )]
[Trait( "Invaders", "Explore" )]
public class ParalyzingFright_Tests {

	[Theory]
	[InlineData(false,4)]
	[InlineData(true, 8)]
	public async Task StopsAllInvaderActions(bool triggerThreshold,int expectedFear) {

		var gs = new SoloGameState();
		var space = gs.Board[2].ScopeSpace;

		// Given: a host of Dahan and Invaders
		string originalTokens = "1C@3,2D@2,5E@1,2T@2";
		space.Given_InitSummary(originalTokens);

		//   And: spirit has some elements
		if(triggerThreshold)
			gs.Spirit.Configure().Elements("2 air,3 earth");

		// When: card played
		await ParalyzingFright.ActAsync(gs.Spirit.Target(space)).AwaitUser(user => {
			if(triggerThreshold)
				user.NextDecision.HasPrompt("Activate Element Threshold?").Choose("Yes");
		}).ShouldComplete(ParalyzingFright.Name);

		//  And: Invaders: Ravage, Build, Explore
		var card = space.BuildInvaderCard();
		await card.When_Ravaging();
		await card.When_Building();
		await card.When_Exploring();

		// Then: none of that happened.
		space.Summary.ShouldBe(originalTokens);

		//  And: 4 fear were generated
		int actualFear = gs.Fear.ActivatedCards.Count * 4 + gs.Fear.EarnedFear;
		actualFear.ShouldBe(expectedFear);
	}

}