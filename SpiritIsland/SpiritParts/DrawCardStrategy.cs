namespace SpiritIsland;

public class DrawCardStrategy(Spirit spirit) {

	public async Task<DrawCardResult> Card(Func<PowerCardDeck, Task<bool>>? forgetCardForMajor = null) {
		PowerCardDeck deck = await DrawFromDeck.SelectPowerCardDeck(_spirit);
		bool forget = (deck.PowerType == PowerType.Major) // is major
			&& (forgetCardForMajor is null || await forgetCardForMajor(deck));
		return await Inner(deck, 4, 1, forget);
	}

	public Task<DrawCardResult> Minor(int numberToDraw = 4, int numberToKeep = 1)
		=> Inner(GameState.Current.MinorCards!, numberToDraw, numberToKeep, false);

	public Task<DrawCardResult> Major(bool forgetCard = true, int numberToDraw = 4, int numberToKeep = 1)
		=> Inner(GameState.Current.MajorCards!, numberToDraw, numberToKeep, forgetCard);

	protected virtual async Task<DrawCardResult> Inner(PowerCardDeck deck, int numberToDraw, int numberToKeep, bool forgetACard) {
		var card = await DrawFromDeck.DrawInner(_spirit, deck, numberToDraw, numberToKeep);
		if( forgetACard )
			await _spirit.Forget.ACard();
		return card;
	}

	readonly protected Spirit _spirit = spirit;
}
