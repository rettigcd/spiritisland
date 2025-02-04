namespace SpiritIsland.Horizons;

class DahanTrustTheWatchers(Spirit spirit) : IDefendSpaces {

	public const string Name = "Dahan Trust the Watchers";
	const string Description = "After one of your Powers adds Defend to a single land, Gather up to 1 Dahan into that land.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	void IDefendSpaces.Defend(Space space, int defense) {
		space.Defend.Add(defense);
		if( Used) return;
		ActionScope.Current.AtEndOfThisAction(scope => spirit.Target(space).GatherUpToNDahan(1) );
		Used = true;
	}
	bool Used {
		get => ActionScope.Current.ContainsKey(Name);
		set => ActionScope.Current[Name] = true;
	}
}