namespace SpiritIsland.JaggedEarth;

[InnatePower("Stars Blaze in the Daytime Sky"), Slow, Yourself]
class StarsBlazeInTheDaytimeSky{

	[InnateTier("4 sun","3 Fear. Gain 1 Energy. Reclaim up to 1 Power Card from play or your discard pile.")]
	static public async Task Option1( Spirit self ) {

		// 3 fear
		self.AddFear(3);

		// Gain 1 energy
		self.Energy++;

		// Reclaim up to 1 power card from play or your discard pile
		var cards = self.InPlay.Union(self.DiscardPile).ToArray();
		if(cards.Length == 0) return;
		var card = await self.SelectPowerCard("Reclaim card", cards, CardUse.Reclaim,Present.Always);

		self.Reclaim(card);	// reclaim it now

	}

}