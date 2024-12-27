namespace SpiritIsland.Horizons;

class OfferNoFirmFoundations(Spirit spirit, Func<Space,(int,HumanTokenClass)> orig) {

	static public SpecialRule Rule => new SpecialRule(
		"Offer No Firm Foundations",
		"At your Sacred Site, Build actions add Explorer instead of Town/City."
	);

	static public void Init(Spirit spirit, GameState gs) {
		// Replace the piece picker
		var oneSpaceBuilder = gs.InvaderDeck.Build.Engine.OneSpacebuilder;
		oneSpaceBuilder.BuildUnitPicker = new OfferNoFirmFoundations(spirit, oneSpaceBuilder.BuildUnitPicker).NoBuildingsOnMud;
	}

	public (int, HumanTokenClass) NoBuildingsOnMud(Space space) {
		var (count,type) = orig(space);
		if(spirit.Presence.IsSacredSite(space))
			type = Human.Explorer;
		return (count, type);
	}
}
