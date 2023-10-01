namespace SpiritIsland.JaggedEarth;

class EnthrallTheForeignExplorers : SpiritPresenceToken, ISkipRavages {

	static public readonly SpecialRule Rule = new SpecialRule( 
		"Enthrall the Foreign Explorers", 
		"For each of your presence in a land, ignore up to 2 explorer during the Ravage Step and any Ravage Action."
	);

	public EnthrallTheForeignExplorers( Spirit self ):base(self) {}

	public UsageCost Cost => UsageCost.Free; // doesn't cost anything to use.

	public async Task<bool> Skip( SpaceState space ) {
		int maxRemovable = _spirit.Presence.CountOn( space ) * 2;

		int explorerCount = space.Sum( Human.Explorer );

		List<HumanToken> explorerTypes = space.OfHumanClass( Human.Explorer ).ToList();

		int removableCount = System.Math.Min( maxRemovable, explorerCount );
		int removed = 0;
		while(removed < removableCount) {
			// Select type to not participate (strifed / non-strifed)
			HumanToken explorerTypeToNotParticipate = explorerTypes.Count == 1 ? explorerTypes[0]
				: ( await _spirit.Gateway.Decision( Select.TokenFrom1Space.TokenToRemove( space.Space, 1, explorerTypes.ToArray(), Present.Done ) ))?.Token.AsHuman();
			if(explorerTypeToNotParticipate == null) break;

			var countToNotParticipate = await _spirit.SelectNumber( $"{space.Space.Text}: # of {explorerTypeToNotParticipate} to not participate in Ravage.", space[explorerTypeToNotParticipate], 0 );

			if(countToNotParticipate > 0)
				space.RavageBehavior.NotParticipating[explorerTypeToNotParticipate] += countToNotParticipate;

			explorerTypes.Remove( explorerTypeToNotParticipate ); // don't let them select the same type twice and over-count the # of non-participants of that type.

			removed += countToNotParticipate;
		}

		return false; // does not stop ravage ever
	}

}