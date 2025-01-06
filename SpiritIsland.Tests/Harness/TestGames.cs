namespace SpiritIsland.Tests;

public class TestGames {
	static readonly public GameBuilder GameBuilder = new GameBuilder(
		new Basegame.GameComponentProvider(),
		new BranchAndClaw.GameComponentProvider(),
		new FeatherAndFlame.GameComponentProvider(),
		new JaggedEarth.GameComponentProvider()
	);
}