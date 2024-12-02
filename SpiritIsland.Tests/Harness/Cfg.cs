#nullable enable
namespace SpiritIsland.Tests;

static public class Cfg {

	/// <summary>
	/// Uses ConfigurableTestFixture to construct the game.
	/// </summary>
	static public GameState BuildGame(this GameConfiguration cfg) => ConfigurableTestFixture.GameBuilder.BuildGame(cfg);

	/// <summary>
	/// Uses ConfigurableTestFixture to construct the game.
	/// </summary>
	static public GameState BuildShell(this GameConfiguration cfg) => ConfigurableTestFixture.GameBuilder.BuildShell(cfg);

}
