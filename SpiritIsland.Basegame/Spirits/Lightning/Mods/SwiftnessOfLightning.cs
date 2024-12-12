
namespace SpiritIsland.Basegame;

sealed class SwiftnessOfLightning(Spirit spirit) : RunSlowCardsAsFast(spirit) {

	static public readonly SpecialRule Rule = new SpecialRule(
		"Swiftness of Lightning",
		"For every Simple air you have, you may use 1 Slow Power as if it were fast"
	);

	protected override int AllowedCount => _spirit.Elements.Get(Element.Air);
}
