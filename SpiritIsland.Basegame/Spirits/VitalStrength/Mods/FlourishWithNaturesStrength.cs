
namespace SpiritIsland.Basegame;

class FlourishWithNaturesStrength(Spirit spirit) : DrawCardStrategy(spirit) {

	public const string Name = "Flourish with Nature's Strength";
	const string Description = "After you gain a Major Power, gain a Minor Power.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	static public void InitAspect(Spirit spirit) {
		spirit.Draw = new FlourishWithNaturesStrength(spirit);
	}

	// Could be implemented with an Event instead of inheritance
	protected override async Task<DrawCardResult> Inner(PowerCardDeck deck, int numberToDraw, int numberToKeep, bool forgetACard) {
		var result = await base.Inner(deck, numberToDraw, numberToKeep, forgetACard);

		// After you gain a Major Power
		var gs = GameState.Current;
		if(deck == gs.MajorCards)
			// gain a Minor Power.
			await base.Inner(gs.MinorCards!,4,1,false);

		return result;
	}

}