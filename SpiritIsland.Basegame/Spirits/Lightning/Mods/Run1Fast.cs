
namespace SpiritIsland.Basegame;

sealed class Run1Fast(Spirit spirit) : RunSlowCardsAsFastMod(spirit) {
	protected override int AllowedCount => 1;
}
