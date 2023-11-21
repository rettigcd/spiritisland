namespace SpiritIsland.NatureIncarnate;

public class DiscardCard : SpiritAction {
	readonly Func<Spirit,IEnumerable<PowerCard>> _cardOptionSelector;

	public DiscardCard( string description, Func<Spirit, IEnumerable<PowerCard>> cardOptionSelector ) 
		: base(description)
	{
		_cardOptionSelector = cardOptionSelector;
	}

	public override async Task ActAsync( SelfCtx ctx ){
		Spirit spirit = ctx.Self;
		PowerCard card = await spirit.Select<PowerCard>( new A.PowerCard(
			$"Select card to discard",
			SingleCardUse.GenerateUses( CardUse.Discard, _cardOptionSelector( spirit ) ),
			Present.Always
		) );
		if(card != null) {

			// Remove from inPlay or hand (should only be in hand...)
			var match = new[] { spirit.InPlay, spirit.Hand }
				.Single( deck => deck.Contains( card ) );
			match.Remove( card );

			// add to discard
			spirit.DiscardPile.Add( card );
		}
	}
}