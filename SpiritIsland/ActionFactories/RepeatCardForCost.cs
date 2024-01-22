namespace SpiritIsland;

// ** NOTE **
// Repeat != Replay
// Repeat => a card in play may be activated a 2nd time (already in play so no new elements added)
// Play => a card in Hand may be played (new elements added)

/// <summary>
/// A card already in Play, may be Activated an additional time. (no new elements)
/// </summary>
public class RepeatCardForCost( params string[] exclude ) : IActionFactory {

	readonly protected string[] exclude = exclude;

	public bool CouldActivateDuring( Phase speed, Spirit _ ) 
		=> speed == Phase.Fast || speed == Phase.Slow;

	public virtual string Name => "Repeat Card for Cost";

	public string Text => Name;

	public async Task ActivateAsync( Spirit self ) {

		PowerCard[] options = GetCardOptions( self, GameState.Current.Phase );
		if(options.Length == 0) return;

		PowerCard powerCard = await self.SelectPowerCard( "Select card to repeat", options, CardUse.Repeat, Present.Always );
		if(powerCard == null) return;

		self.Energy -= powerCard.Cost;
		self.AddActionFactory( powerCard );

	}


	public virtual PowerCard[] GetCardOptions( Spirit self, Phase phase ) {
		int maxCardCost = self.Energy;
		PowerCard[] options = self.UsedActions.OfType<PowerCard>() // can't use Discard pile because those cards are from prior rounds.
			.Where( card => !exclude.Contains(card.Name) )
			.Where( card => card.CouldActivateDuring( phase, self ) )
			.Where( card => card.Cost <= maxCardCost )
			.ToArray();
		return options;
	}
}

public class RepeatCheapestCardForCost( params string[] exclude ) : RepeatCardForCost(exclude) {
	public override PowerCard[] GetCardOptions( Spirit self, Phase phase ) {
		return [
			.. base.GetCardOptions(self,phase)
				.GroupBy( pc => pc.Cost )  // group with lowest cost
				.OrderBy( grp => grp.Key )
				.First()
		];
	}
}
