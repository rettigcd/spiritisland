namespace SpiritIsland.JaggedEarth;

class ThePastReturnsAgain {

	[SpiritCard( "The Past Returns Again", 0, Element.Sun, Element.Moon ), Fast, Yourself]
	[Instructions( "Cost to Use: N Time, and Spirits jointly pay N Energy (where N = # of players). Swap the top card of the Invader Deck with a card in the Invader discard that is within 1 Invader Stage of it. (The discarded card stays face-down. You can't swap cards that don't exist.)" ), Artist( Artists.LucasDurham )]
	static public async Task ActAsync( SelfCtx ctx ) {

		if(ctx.Self is not FracturedDaysSplitTheSky frac) return;

		// Cost to Use: N Time, and Spirits jointly pay N Energy (where N = # of players).
		var gs = GameState.Current;
		int cost = gs.Spirits.Length;

		if(frac.Time < cost) return;

		int existingEnergy = gs.Spirits.Sum( s => s.Energy );
		if(existingEnergy < cost) return;

		if(! await GetEnergyFromEachSpirit( ctx, gs.Spirits.Length ) ) return;

		await frac.SpendTime( cost );

		// Swap the top card of the Invader Deck with a card in the Invader discard that is within 1 Invader Stage of it.
		var deck = gs.InvaderDeck;
		var newCard = deck.Explore.Cards.First();
		int stageOfTopCard = newCard.InvaderStage;
		var options = deck.Discards.Where( d => System.Math.Abs( d.InvaderStage - stageOfTopCard ) <= 1 ).ToArray();
		// You can't swap cards that don't exist.
		if(options.Length == 0) return;

		var oldCard = await ctx.SelectAsync( new A.TypedDecision<InvaderCard>(
			"Select card to return to top of Invader deck", options, Present.Always
		) );

		// Replace New with Old
		deck.Explore.Cards[0] = oldCard;
		// Replace Old with New
		deck.Discards.Remove( oldCard );
		deck.Discards.Add( newCard ); // face down

		// The Discarded card stays face-down.
	}

	// Reuse this for Events
	static async Task<bool> GetEnergyFromEachSpirit( SelfCtx ctx, int total ) {
		int remaining = total;
		var pledge = new CountDictionary<Spirit>();
		do {
			foreach(var s in GameState.Current.Spirits) {
				int max = Math.Min( remaining, s.Energy - pledge[s] );
				if(max == 0) continue;
				var payOptions = new List<ItemOption<int>>();
				int i = max; while(0 <= i) { payOptions.Add( new ItemOption<int>( i-- ) ); }
				var x = await s.Select( new A.TypedDecision<ItemOption<int>>( $"Contribute Energy? ({remaining} of {total} outstanding)", payOptions, Present.Always ) );
				pledge[s] -= x.Item;
				remaining -= x.Item;
				if(remaining == 0) break;
			}
		} while(0 < remaining && await ctx.Self.UserSelectsFirstText( $"{remaining} of {total} remaining.", "Pass the hat again.", "No, this isn't going to work." ));

		if(0 < remaining) return false; // we didn't make it

		// Pay pledge
		foreach(Spirit spirit in GameState.Current.Spirits)
			spirit.Energy -= pledge[spirit];

		return true;
	}
}