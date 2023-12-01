namespace SpiritIsland.NatureIncarnate;

public class DiscardCard : SpiritAction {
	readonly Func<Spirit,IEnumerable<PowerCard>> _cardOptionSelector;

	public DiscardCard( string description, Func<Spirit, IEnumerable<PowerCard>> cardOptionSelector ) 
		: base(description)
	{
		_cardOptionSelector = cardOptionSelector;
	}

	public override async Task ActAsync( Spirit self ){
		PowerCard card = await self.SelectAsync<PowerCard>( new A.PowerCard(
			$"Select card to discard",
			SingleCardUse.GenerateUses( CardUse.Discard, _cardOptionSelector( self ) ),
			Present.Always
		) );
		if(card != null) {

			// Remove from inPlay or hand (should only be in hand...)
			var match = new[] { self.InPlay, self.Hand }
				.Single( deck => deck.Contains( card ) );
			match.Remove( card );

			// add to discard
			self.DiscardPile.Add( card );
		}
	}
}