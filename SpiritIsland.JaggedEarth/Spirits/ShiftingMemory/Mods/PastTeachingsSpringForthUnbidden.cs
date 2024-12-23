
namespace SpiritIsland.JaggedEarth;

class PastTeachingsSpringForthUnbidden(Spirit spirit) : DrawCardStrategy(spirit) {

	public const string Name = "Past Teachings Spring Forth Unbidden";
	const string Description = "When you gain Power Cards, draw 2 fewer cards (min. 2) and gain 1 more of them (normally draw 2 cards and gain both). (Forget only one Power Card when gaining Major Powers.)";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	static public void InitAspect(Spirit spirit) {
		spirit.Draw = new PastTeachingsSpringForthUnbidden(spirit);
	}


	protected override Task<DrawCardResult> Inner(PowerCardDeck deck, int numberToDraw, int numberToKeep, bool forgetACard) {
		numberToDraw = Math.Max(numberToDraw-2,2);
		++numberToKeep;
		return base.Inner(deck, numberToDraw, numberToKeep, forgetACard);
	}

}
