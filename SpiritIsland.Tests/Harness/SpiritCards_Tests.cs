namespace SpiritIsland.Tests; 

public class SpiritCards_Tests {

	protected SpiritCards_Tests(Spirit spirit ) {
		this._spirit = spirit;
		this.User = new VirtualUser( spirit );
	}

	protected GameState _gameState;
	protected PowerCard _card;// !!! check if anything other than WhenPlayingCard is using this.
	protected Spirit _spirit;
	protected VirtualUser User { get; }

	protected void Given_PurchasedCard(string cardName) {
		_card = _spirit.Hand.Single( c => c.Name == cardName );
		PlayCard();
	}

	protected void PlayCard() {
		_spirit.PlayCard( _card );
	}

	protected void Given_GameWithSpirits(params Spirit[] spirits) {
		var boards = new List<Board> { Board.BuildBoardA() };
		if(0 < spirits.Length)
			boards.Add( Board.BuildBoardB( Boards.Attach1 ) );

		_gameState = new GameState( spirits, boards.ToArray() );
	}

	protected static void Given_PurchasedFakePowercards(Spirit otherSpirit, int expectedEnergyBonus) {
		for (int i = 0; i < expectedEnergyBonus; ++i) {
			var otherCard = PowerCard.For(typeof(SpiritIsland.Basegame.GiftOfLivingEnergy));
			otherSpirit.InPlay.Add(otherCard);
			otherSpirit.AddActionFactory(otherCard);
		}
	}

	static protected void Assert_CardStatus( PowerCard card, int expectedCost, Phase expectedSpeed, string expectedElements ) {
		Assert.Equal( expectedCost, card.Cost );
		Assert.Equal( expectedSpeed, card.DisplaySpeed );

		var cardElements = card.Elements.BuildElementString(false);
		Assert.Equal( expectedElements, string.Join("",cardElements));

	}

	protected void Assert_CardIsReady( PowerCard card, Phase speed ) {
		Assert.Contains(card, _spirit.GetAvailableActions(speed).OfType<PowerCard>().ToList());
	}

	protected Task When_PlayingCard() => _card.ActivateAsync( _spirit.Bind() );

}

