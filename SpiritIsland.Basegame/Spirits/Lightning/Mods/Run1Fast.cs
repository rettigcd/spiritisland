
namespace SpiritIsland.Basegame;

sealed class Run1Fast(Spirit spirit) : RunSlowCardsAsFast(spirit) {
	protected override int AllowedCount => 1;
}
