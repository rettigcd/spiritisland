namespace SpiritIsland;

public class DrawFromDeck {

	public static async Task<PowerCardDeck> SelectPowerCardDeck( Spirit spirit ) {
		PowerType powerType = await spirit.Select( new A.DeckToDrawFrom( PowerType.Minor, PowerType.Major ) );
		GameState gs = GameState.Current;
		return powerType == PowerType.Minor ? gs.MinorCards : gs.MajorCards;
	}

	static public async Task<DrawCardResult> DrawInner( Spirit spirit, PowerCardDeck deck, int numberToDraw, int numberToKeep ) {
		List<PowerCard> candidates = deck.Flip(numberToDraw);

		var selectedCards = new List<PowerCard>();
		while(numberToKeep-- > 0) {
			var selected = await TakeCard( spirit, candidates );
			selectedCards.Add( selected );
		}

		deck.Discard( candidates );
		return new DrawCardResult( selectedCards[0].PowerType ){
			SelectedCards = selectedCards.ToArray(),
			Rejected = candidates
		};
	}

	public static async Task<PowerCard> TakeCard( Spirit spirit, List<PowerCard> flipped ) {
		string powerType = flipped.Select(x=>x.PowerType.Text ).Distinct().Join("/");
		var selectedCard = await spirit.SelectPowerCard( $"Select {powerType} Power Card", flipped, CardUse.AddToHand, Present.Always );
		spirit.Hand.Add( selectedCard );
		flipped.Remove( selectedCard );
		return selectedCard;
	}

}