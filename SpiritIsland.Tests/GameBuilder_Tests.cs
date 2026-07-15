namespace SpiritIsland.Tests;

public class GameBuilder_Tests {

	[Fact]
	public void BuildsGames_Has9FearCards() {
		var gs = new GameConfiguration().ConfigSpirits(RiverSurges.Name).ConfigBoards("A").BuildGame();
		gs.Fear.CardsPerLevel_Remaining.Sum().ShouldBe(9);
	}

	[Fact]
	public void BuildGame_RecordsAppliedAspects() {
		var gs = new GameConfiguration()
			.ConfigSpirits(SpiritIsland.Basegame.Bringer.Name)
			.ConfigAspects(SpiritIsland.Basegame.Violence.ConfigKey)
			.ConfigBoards("A")
			.BuildGame();

		Spirit spirit = gs.Spirits.Single();

		spirit.AppliedAspects.ShouldBe( [SpiritIsland.Basegame.Violence.ConfigKey] );
	}

}