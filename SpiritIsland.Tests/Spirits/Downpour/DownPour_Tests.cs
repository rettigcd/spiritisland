namespace SpiritIsland.Tests.Spirits.DownpourNS;

public class DownPour_Tests {

	[Trait("SpecialRule","DrenchTheLandscape")]
	[Fact]
	public async Task TargetLandUsingPresenceAsWetlands() {

		var gs = new SoloGameState( new DownpourDrenchesTheWorld(), Boards.D);
		gs.Initialize();
		var spirit = gs.Spirit;
		var board = gs.Board;

		// Given: 2 presence on non-wetland
		var sourceSpace = board[7];
		sourceSpace.IsSand.ShouldBeTrue(); // make sure we are on sands
		spirit.Given_IsOn(sourceSpace,2);

		// Given: 1 explorer on target
		var target = board[6];
		target.Given_HasTokens("1E@1");

		// When: play card
		await using var actionScope = ActionScope.Start_NoStartActions( ActionCategory.Spirit_Growth );
		await PowerCard.ForDecorated(DarkSkiesLooseAStingingRain.ActAsync).ActivateAsync( spirit).AwaitUser(user => {
			// Then: can target out of 2-presence non wetland
			target.IsJungle.ShouldBeTrue();
			user.NextDecision.Choose(target.Label);
			// Then:
			user.NextDecision.Choose("Done"); // don't push
		}).ShouldComplete();

	}


}
