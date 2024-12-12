
namespace SpiritIsland.Basegame;

sealed class SwiftnessOfLightning(Spirit spirit) : RunSlowCardsAsFast(spirit) {

	public const string Name = "Swiftness of Lightning";
	const string Description = "For every Simple air you have, you may use 1 Slow Power as if it were fast";

	static public readonly SpecialRule Rule = new SpecialRule( Name, Description );

	protected override int AllowedCount => _spirit.Elements.Get(Element.Air);
}
