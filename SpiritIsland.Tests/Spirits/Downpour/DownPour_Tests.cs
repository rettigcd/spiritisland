namespace SpiritIsland.Tests;

public class DownPour_Tests {

	readonly ConfigurableTestFixture fxt = new ConfigurableTestFixture();

	[Trait("SpecialRule","DrenchTheLandscape")]
	[Fact]
	public async Task TargetLandUsingPresenceAsWetlands() {
		
		// Given: Downpour
		fxt.Spirit = new DownpourDrenchesTheWorld();
		fxt.Board = Board.BuildBoardD();
		fxt.GameState.Initialize(); // 

		// Given: 2 presence on non-wetland
		var sourceSpace = fxt.Board[7];
		sourceSpace.IsSand.ShouldBeTrue(); // make sure we are on sands
		fxt.InitPresence(sourceSpace,2);

		// Given: 1 explorer on target
		var target = fxt.Board[6];
		fxt.InitTokens(target,"1E@1");

		// When: play card
		var card = PowerCard.For<DarkSkiesLooseAStingingRain>();
		await using var actionSCope = ActionScope.Start_NoStartActions( ActionCategory.Spirit_Growth );
		Task task = card.ActivateAsync( fxt.SelfCtx );
		task.IsCompletedSuccessfully.ShouldBeFalse();

		// Then: can target out of 2-presence non wetland
		target.IsJungle.ShouldBeTrue();
		fxt.Choose(target.Text);

		// Then:
		fxt.Choose( "Done" ); // don't push

		await task.ShouldComplete();


	}


}
