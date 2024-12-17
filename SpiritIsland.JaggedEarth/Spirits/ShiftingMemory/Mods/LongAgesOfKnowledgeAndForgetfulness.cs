namespace SpiritIsland.JaggedEarth;

class LongAgesOfKnowledgeAndForgetfulness(Spirit s) : ForgettingStrategy(s) {

	public const string Name = "Long Ages of Knowledge and Forgetfulness";
	const string Description = "When you would Forget a Power Card from your hand, you may instead discard it.";

	static public SpecialRule Rule => new SpecialRule( Name, Description );

	public override async Task<PowerCard?> ACard(IEnumerable<PowerCard>? restrictedOptions = null, Present present = Present.Always) {
		IEnumerable<SingleCardUse> options = SingleCardUse.GenerateUses(CardUse.Discard, _spirit.InPlay.Union(_spirit.Hand))
			.Union(SingleCardUse.GenerateUses(CardUse.Forget, _spirit.DiscardPile))
			.Where(u => restrictedOptions == null || restrictedOptions.Contains(u.Card));

		var decision = new A.PowerCard("Select card to forget or discard", options, present);
		PowerCard? cardToForgetOrDiscard = await _spirit.SelectAsync(decision);
		if( cardToForgetOrDiscard is not null )
			ThisCard(cardToForgetOrDiscard);
		return cardToForgetOrDiscard is not null && !_spirit.DiscardPile.Contains(cardToForgetOrDiscard)
			? cardToForgetOrDiscard // card not in discard pile, must have been forgotten
			: null;
	}

	/// <summary>
	/// If in hand, allows discarding instead of forgetting.
	/// </summary>
	public override void ThisCard(PowerCard card) {

		// (Source-1) Purchased / Active
		if( _spirit.InPlay.Contains(card) ) {

			_spirit.Elements.Remove(card.Elements); // lose elements from forgotten card

			_spirit.InPlay.Remove(card);
			_spirit.DiscardPile.Add(card);
			return;
		}

		if( _spirit.Hand.Remove(card) ) {
			_spirit.DiscardPile.Add(card);
			return;
		}

		if( _spirit.DiscardPile.Contains(card) ) {
			base.ThisCard(card);
			return;
		}

		throw new System.Exception("Can't find card to forget:" + card.Title);
	}

}