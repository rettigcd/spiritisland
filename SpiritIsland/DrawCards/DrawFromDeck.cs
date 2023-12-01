namespace SpiritIsland;

public class DrawFromDeck {

	static public async Task<PowerCardDeck> SelectPowerCardDeck( Spirit spirit ) {
		PowerType powerType = await spirit.SelectAsync( new A.DeckToDrawFrom( PowerType.Minor, PowerType.Major ) );
		return powerType == PowerType.Minor ? GameState.Current.MinorCards : GameState.Current.MajorCards;
	}

	/// <summary>
	/// Does the "Gain Power Card" action - Adds Selected Cards to players hand.
	/// </summary>
	static public async Task<DrawCardResult> DrawInner( Spirit spirit, PowerCardDeck deck, int numberToDraw, int numberToKeep ) {
		List<PowerCard> candidates = deck.Flip(numberToDraw);

		var selectedCards = new List<PowerCard>();
		while(0 < numberToKeep--) {
			PowerCard selectedCard = await PickOutCard( spirit, candidates );
			selectedCards.Add( selectedCard );
		}

		spirit.Hand.AddRange( selectedCards );
		deck.Discard( candidates );

		return new DrawCardResult( selectedCards[0].PowerType ){
			SelectedCards = selectedCards.ToArray(),
			Rejected = candidates
		};
	}

	/// <summary>
	/// (Helper)
	/// User selects 1 card from the List, 
	/// Removes it from the List, AND
	/// RETURNS it.
	/// </summary>
	static public async Task<PowerCard> PickOutCard( Spirit spirit, List<PowerCard> flipped ) {
		string powerType = flipped.Select(x=>x.PowerType.Text ).Distinct().Join("/");
		PowerCard selectedCard = await spirit.SelectPowerCard( $"Select {powerType} Power Card", flipped, CardUse.AddToHand, Present.Always );
		flipped.Remove( selectedCard );
		return selectedCard;
	}

}