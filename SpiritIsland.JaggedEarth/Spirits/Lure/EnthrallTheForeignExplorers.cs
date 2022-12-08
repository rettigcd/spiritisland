namespace SpiritIsland.JaggedEarth;

class EnthrallTheForeignExplorers : SpiritPresenceToken, ISkipRavages {

	static public readonly SpecialRule Rule = new SpecialRule( 
		"Enthrall the Foreign Explorers", 
		"For each of your presence in a land, ignore up to 2 explorer during the Ravage Step and any Ravage Action."
	);

	readonly Spirit _self;
	public EnthrallTheForeignExplorers(Spirit self ) { _self = self; }

	public UsageCost Cost => UsageCost.Free; // doesn't cost anything to use.

	public async Task<bool> Skip( GameState gameState, SpaceState space ) {
		int maxRemovable = _self.Presence.CountOn( space ) * 2;

		int explorerCount = space.Sum( Invader.Explorer );

		var explorerTypes = space.OfType( Invader.Explorer ).ToList();

		int removableCount = System.Math.Min( maxRemovable, explorerCount );
		int removed = 0;
		while(removed < removableCount) {
			// Select type to not participate (strifed / non-strifed)
			var explorerTypeToNotParticipate = explorerTypes.Count == 1 ? explorerTypes[0]
				: await _self.Gateway.Decision( Select.TokenFrom1Space.TokenToRemove( space.Space, 1, explorerTypes.ToArray(), Present.Done ) );
			if(explorerTypeToNotParticipate == null) break;

			var countToNotParticipate = await _self.SelectNumber( $"{space.Space.Text}: # of {explorerTypeToNotParticipate} to not participate in Ravage.", space[explorerTypeToNotParticipate], 0 );

			if(countToNotParticipate > 0)
				gameState.ModifyRavage( space.Space, cfg => cfg.NotParticipating[explorerTypeToNotParticipate] += countToNotParticipate );

			explorerTypes.Remove( explorerTypeToNotParticipate ); // don't let them select the same type twice and over-count the # of non-participants of that type.

			removed += countToNotParticipate;
		}

		return false; // does not stop ravage ever
	}

}