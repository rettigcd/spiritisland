namespace SpiritIsland.JaggedEarth;

class EnthrallTheForeignExplorers : SpiritPresenceToken, IConfigRavagesAsync {

	static public readonly SpecialRule Rule = new SpecialRule( 
		"Enthrall the Foreign Explorers", 
		"For each of your presence in a land, ignore up to 2 explorer during the Ravage Step and any Ravage Action."
	);

	public EnthrallTheForeignExplorers( Spirit self ):base(self) {}

	async Task IConfigRavagesAsync.ConfigAsync( SpaceState space ) {
		await space.SourceSelector
			.AddGroup( Self.Presence.CountOn( space ) * 2, Human.Explorer )
			.SelectFightersAndSitThemOut( Self );
	}

}
