namespace SpiritIsland.JaggedEarth;

class ThePastReturnsAgain {

	[SpiritCard( "The Past Returns Again", 0, Element.Sun, Element.Moon ), Fast, Yourself]
	static public async Task ActAsync( SelfCtx ctx ) {

		if( ctx.Self is not FracturedDaysSplitTheSky frac) return;

		// Cost to Use: N Time, and Spirits jointly pay N Energy (where N = # of players).
		int cost = ctx.GameState.Spirits.Length;

		if(frac.Time < cost) return;

		// !!! Implementing this as 1 spirit, but once we have >1 player, switch to all spirits can pay
		if(frac.Energy < cost) return;

		frac.Energy -= cost;
		await frac.SpendTime( cost );

		// Swap the top card of the Invader Deck with a card in the Invader discard that is within 1 Invader Stage of it.
		var deck = ctx.GameState.InvaderDeck;
		var newCard = deck.Explore.Cards.First();
		int stageOfTopCard = newCard.InvaderStage;
		var options = deck.Discards.Where(d=> System.Math.Abs(d.InvaderStage-stageOfTopCard)<=1).ToArray();
		// You can't swap cards that don't exist.
		if(options.Length == 0) return;

		var oldCard = await ctx.Decision(new Select.TypedDecision<IInvaderCard>(
			"Select card to return to top of Invader deck", options, Present.Always
		));

		// Replace New with Old
		deck.Explore.Cards[0] = oldCard;
		// Replace Old with New
		deck.Discards.Remove( oldCard );
		deck.Discards.Add( newCard ); // face down

		// The Discarded card stays face-down.
		// !!! deck / card has no knowledge of face-up/face-down, if spirit plays this card again, they will see the 'face-down' card
	}

}

// !!! draw the stage of the upcoming explorer card
