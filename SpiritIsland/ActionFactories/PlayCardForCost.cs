namespace SpiritIsland;

// !!! Make this a SpiritAction, not an IActionFactory

/// <summary>
/// A card in spirit's Hand, may be Played (triggering its elements to be added)
/// </summary>
public class PlayCardForCost( Present present = Present.Always ) : IActionFactory {
	public bool CouldActivateDuring( Phase speed, Spirit _ ) 
		=> speed == Phase.Fast || speed == Phase.Slow;

	string IOption.Text => Title;

	public string Title => "Play Card for Cost";

	public async Task ActivateAsync( Spirit self ) {

		int maxCardCost = self.Energy;
		var options = self.Hand.OfType<PowerCard>()
			.Where(card=>card.Cost<=maxCardCost)
			.ToArray();
		if(options.Length == 0) return;

		PowerCard powerCard = await self.SelectPowerCard( "Select card to play", 1, options.Where( x => x.Cost <= maxCardCost ), CardUse.Play, present );
		if(powerCard != null)
			self.PlayCard( powerCard );
	}
}

