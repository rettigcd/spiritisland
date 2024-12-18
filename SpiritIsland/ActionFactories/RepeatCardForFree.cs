namespace SpiritIsland;

/// <summary>
/// Card is repeated, but does not cost energy
/// </summary>
public class RepeatCardForFree : IActionFactory {

	#region constructors

	/// <summary> Replay any discard card for free. </summary>
	public RepeatCardForFree() {
		this.maxCost = int.MaxValue;
	}

	/// <summary> Replay discard card for free limited by maxCost. </summary>
	public RepeatCardForFree( int maxCost ) {
		this.maxCost = maxCost;
	}

	#endregion

	public bool CouldActivateDuring( Phase speed, Spirit _ ) 
		=> speed == Phase.Fast || speed == Phase.Slow;

	string IOption.Text => Title;

	public string Title => "Replay Card" + Suffix;
	string Suffix => maxCost == int.MaxValue ? "" : $" (max cost:{maxCost})";

	public async Task ActivateAsync(Spirit self) {

		var options = self.UsedActions.OfType<PowerCard>() // not using Discard Pile because those cards are from previous rounds
			.Where(card=>card.Cost <= maxCost)
			.Where(card=> card.CouldActivateDuring(GameState.Current.Phase,self)) 
			.ToArray(); 


		if(options.Length == 0) return;

		PowerCard? factory = await self.SelectPowerCard( "Select card to repeat", 1, options, CardUse.Repeat, Present.Done );
		if(factory is null) return;

		self.AddActionFactory( factory );

	}


	readonly int maxCost;
}
