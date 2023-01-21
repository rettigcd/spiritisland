namespace SpiritIsland.FeatherAndFlame;
internal class DiscardPowerCards : GrowthActionFactory {

	public DiscardPowerCards(int count ) { this.count = count; }

	public override async Task ActivateAsync( SelfCtx ctx ) {

		var spirit = ctx.Self;
		var hand = spirit.Hand;
		var inPlay = spirit.InPlay;
		for(int i = 0; i < count; ++i) {
			IEnumerable<SingleCardUse> options = SingleCardUse.GenerateUses( CardUse.Discard, inPlay.Union( hand ) );
			var decision = new Select.PowerCard( $"Select card to discard ({i+1}of{count})", options, Present.Always );
			PowerCard card = await ctx.Decision( decision );
			if(card != null) {
				// (Source-1) Purchased / Active
				if(inPlay.Contains( card )) {
					foreach(var el in card.Elements) spirit.Elements[el.Key] -= el.Value;// lose elements from forgotten card
					inPlay.Remove( card );
					spirit.DiscardPile.Add( card );
				} else if(hand.Contains( card )) {
					hand.Remove( card );
					spirit.DiscardPile.Add( card );
				} else
					throw new InvalidOperationException("card must be in hand or in play.");
			}
		}
	}

	readonly int count;

}

