namespace SpiritIsland.Tests;

public class GameBuilder_Tests {

	[Fact]
	public void BuildsGames_Has9FearCards() {
		var gs = new GameConfiguration().ConfigSpirits(RiverSurges.Name).ConfigBoards("A").BuildGame();
		gs.Fear.CardsPerLevel_Remaining.Sum().ShouldBe(9);
	}
	
}