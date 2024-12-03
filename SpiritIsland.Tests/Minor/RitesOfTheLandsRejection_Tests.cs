namespace SpiritIsland.Tests.Minor;

public class RitesOfTheLandsRejection_Tests {

	[Trait( "Invaders", "Build" )]
	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineData(false, 1, "1E@1,1T@2")] // Stops none
	[InlineData(true, 1, "1E@1")] // Stops 1 build
	[InlineData(true, 5, "1E@1")] // Stops all builds
	public async void SingleBuild(bool playsCard, int numberOfbuilds, string result) {

		var gs = new SoloGameState();
		var space = gs.Board[6].ScopeSpace;

		// Given: 1 explorer on it
		space.Given_InitSummary("1E@1");

		//  When: Rites is played
		if( playsCard )
			await RitesOfTheLandsRejection.ActAsync(gs.Spirit.Target(space)); // No Dahan, Push-Dahan option not presented

		//   And: Build occurs
		while( 0 < numberOfbuilds-- )
			await space.When_CardBuilds();

		//  Then: expected invaders
		space.Summary.ShouldBe(result);
	}

}
