namespace SpiritIsland.JaggedEarth;
	
// !!! When we add Event Cards, add to this power

[InnatePower("Visions of a Shifting Future"), Slow, Yourself]
class VisionsOfAShiftingFuture {

	[InnateTier("1 sun,2 moon,2 air","Look at the top card of either the Invader Deck or the Event Deck.  Return it, then shuffle that deck's top 2 cards.")]
	static public async Task Option1( SelfCtx ctx ) {
		var deck = GameState.Current.InvaderDeck.UnrevealedCards;
		InvaderCard topCard = deck[0];
		await ctx.Self.SelectText(topCard.Text,new string[] {"Shuffle with next top card." },Present.Always);
		deck.RemoveAt(0);
		InsertIntoTop2( ctx.Self, deck, topCard );
	}

	[InnateTier("2 sun,3 moon,2 air","Instead of returning-and-shuffling, you may put the card you looked at on the bottom of its deck.  You may not do this for cards specially placed during Setup.")]
	static public async Task Option2( SelfCtx ctx ) {
		var deck = GameState.Current.InvaderDeck.UnrevealedCards;
		InvaderCard topCard = deck[0];
		deck.RemoveAt(0);

		if( await ctx.Self.UserSelectsFirstText(topCard.Text,new string[] {"Shuffle with next top card.", "Move to bottom of deck." }) )
			InsertIntoTop2( ctx.Self, deck, topCard );
		else
			deck.Add(topCard);
	}

	static void InsertIntoTop2( Spirit spirit, List<InvaderCard> deck, InvaderCard topCard ) {
		int index = ((FracturedDaysSplitTheSky)spirit).NextRandom(2);
		deck.Insert( index, topCard );
	}

}