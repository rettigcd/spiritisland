
namespace SpiritIsland.Basegame;

/// <summary>
/// Mod that inserts Slow cards into the ActionList during the Fast phase based on # of Air elements.
/// </summary>
sealed class SwiftnessOfLightning(Spirit spirit) : RunSlowCardsAsFastMod_EveryRound(spirit) {

	public const string Name = "Swiftness of Lightning";
	const string Description = "For every Simple air you have, you may use 1 Slow Power as if it were fast";
	static public readonly SpecialRule Rule = new SpecialRule( Name, Description );

	protected override int AllowedCount => _spirit.Elements[Element.Air];
}
