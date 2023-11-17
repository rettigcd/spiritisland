namespace SpiritIsland.NatureIncarnate;

internal class DiscardPowerCardWithFire : SpiritAction {

	public DiscardPowerCardWithFire() : base( "Discard a Power Card with fire" ) {}

	public override async Task ActAsync( SelfCtx ctx ) {

		var spirit = ctx.Self;
		var hand = spirit.Hand;
		var inPlay = spirit.InPlay;

		PowerCard card = await ctx.SelectAsync( new A.PowerCard( 
			$"Select card to discard", 
			SingleCardUse.GenerateUses( CardUse.Discard, inPlay.Union( hand ) )
				.Where( u => 0 < u.Card.Elements[Element.Fire]), 
			Present.Always 
		) );
		if(card == null) return; // no cards

		// Remove from inPlay or hand (should only be in hand...)
		if(inPlay.Contains( card ))
			inPlay.Remove( card );
		else if(hand.Contains( card ))
			hand.Remove( card );
		else
			throw new InvalidOperationException( "card must be in hand or in play." );

		// add to discard
		spirit.DiscardPile.Add( card );
	}

}