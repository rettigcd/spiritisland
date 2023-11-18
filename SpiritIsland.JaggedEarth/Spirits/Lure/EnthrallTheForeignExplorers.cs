namespace SpiritIsland.JaggedEarth;

class EnthrallTheForeignExplorers : SpiritPresenceToken, ISkipRavages {

	static public readonly SpecialRule Rule = new SpecialRule( 
		"Enthrall the Foreign Explorers", 
		"For each of your presence in a land, ignore up to 2 explorer during the Ravage Step and any Ravage Action."
	);

	public EnthrallTheForeignExplorers( Spirit self ):base(self) {}

	public UsageCost Cost => UsageCost.Free;

	public async Task<bool> Skip( SpaceState space ) {

		var sourceSelector = new SourceSelector( space )
			.NotRemoving()
			.AddGroup( Self.Presence.CountOn( space ) * 2, Human.Explorer );

		await SitOutRavage.SelectFightersAndSitThemOut( Self, sourceSelector );

		return false; // does not stop ravage ever
	}

}
