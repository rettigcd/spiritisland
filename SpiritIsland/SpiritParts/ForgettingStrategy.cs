#nullable enable
namespace SpiritIsland;

public class ForgettingStrategy(Spirit spirit) {

	public virtual async Task<PowerCard?> ACard(IEnumerable<PowerCard>? options = null, Present present = Present.Always) {

		options ??= GetForgetableCards();

		PowerCard? cardToForget = await _spirit.SelectPowerCard("Select power card to forget", 1, options, CardUse.Forget, present);
		if( cardToForget is not null )
			ThisCard(cardToForget);
		return cardToForget;
	}

	protected virtual IEnumerable<PowerCard> GetForgetableCards()
		=> _spirit.InPlay               // in play
			.Union(_spirit.Hand)        // in Hand
			.Union(_spirit.DiscardPile) // in Discard
			.ToArray();

	public virtual void ThisCard(PowerCard cardToRemove) {
		// A card can be in one of 3 places
		// (1) Purchased / Active
		if( _spirit.InPlay.Contains(cardToRemove) )
			RemoveCardFromPlay(cardToRemove);
		// (2) Unpurchased, still in hand
		_spirit.Hand.Remove(cardToRemove);
		// (3) used, discarded
		_spirit.DiscardPile.Remove(cardToRemove);
	}

	public void RemoveCardFromPlay(PowerCard cardToRemove) {
		_spirit.Elements.Remove(cardToRemove.Elements);  // lose elements from forgotten card
		_spirit.InPlay.Remove(cardToRemove);
	}

	readonly protected Spirit _spirit = spirit;
}
