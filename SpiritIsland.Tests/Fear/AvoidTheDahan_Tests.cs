namespace SpiritIsland.Tests.Fear;

public class AvoidTheDahan_Tests {

	[Trait("Invaders", "Explore")]
	[Theory]
	[InlineData(1, "1T@2", 2, "2D@2,1T@2")]           // 2 Dahan show up after fear, before explore => No Explore
	[InlineData(1, "2D@2,1E@1,1T@2", 0, "2E@1,1T@2")] // Dahan removed during Ravage, => Should Explore
	public async Task TestExplore( int terrorLevel, string initialTokens, int preExploreDahan, string expectedEndingTokens ) {

		var gs = new SoloGameState();
		var space = gs.Board[7].ScopeSpace;

		// Given:
		space.Given_InitSummary(initialTokens);
		await new AvoidTheDahan().ActAsync(terrorLevel);

		//  When:
		space.InitDefault(Human.Dahan, preExploreDahan);
		await space.When_CardExplories();

		//  Then: new Explorer appears
		space.Summary.ShouldBe(expectedEndingTokens);
	}

	[Trait("Invaders", "Build")]
	[Theory]
	[InlineData(2, "3D@2,2T@2", 1, "1C@3,1D@2,2T@2")] // 2 Dahan at fear, 1 Dahan at Build => Build
	[InlineData(2, "3C@1,5D@2,1T@1", 5, "3C@1,5D@2,1T@1")] // No Build
	[InlineData(3, "3D@2,1T@2", 0, "1C@3,1T@2")] // Dahan Counted at Build, not Fear => Build
	public async Task TestBuild(int terrorLevel, string initialTokens, int dahanBeforeBuild, string expectedEndingTokens) {
		var gs = new SoloGameState();
		var space = gs.Island.Boards[0][4].ScopeSpace;

		// Given:
		space.Given_InitSummary(initialTokens);

		//   And: Avoid the Dahan - 3 activated (with 3 dahan present
		await new AvoidTheDahan().ActAsync(terrorLevel);

		//   And: Dahan are removed (maybe via Ravage)
		space.InitDefault(Human.Dahan,dahanBeforeBuild);

		// When: Build
		await space.When_CardBuilds();

		// Then: invaders Build.
		space.Summary.ShouldBe(expectedEndingTokens);
	}

}
