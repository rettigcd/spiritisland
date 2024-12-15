namespace SpiritIsland.JaggedEarth;

class PastTeachingsSpringForthUnbidden {
	public const string Name = "Past Teachings Spring Forth Unbidden";
	const string Description = "When you gain Power Cards, draw 2 fewer cards (min. 2) and gain 1 more of them (normally draw 2 cards and gain both). (Forget only one Power Card when gaining Major Powers.)";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	static public void InitAspect(Spirit spirit) {
		// !!!! 
	}
}
