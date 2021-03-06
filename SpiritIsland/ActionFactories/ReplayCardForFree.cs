namespace SpiritIsland;

/// <summary>
/// Card is played again, but does not cost energy
/// </summary>
public class ReplayCardForFree : IActionFactory {

	#region constructors

	/// <summary> Replay any discard card for free. </summary>
	public ReplayCardForFree() {
		this.maxCost = int.MaxValue;
	}

	/// <summary> Replay discard card for free limited by maxCost. </summary>
	public ReplayCardForFree( int maxCost ) {
		this.maxCost = maxCost;
	}

	#endregion

	public bool CouldActivateDuring( Phase speed, Spirit _ ) 
		=> speed == Phase.Fast || speed == Phase.Slow;

	public string Name => "Replay Card" + Suffix;
	string Suffix => maxCost == int.MaxValue ? "" : $" [max cost:{maxCost}]";
	public string Text => Name;

	public async Task ActivateAsync(SelfCtx ctx) {

		var options = ctx.Self.UsedActions.OfType<PowerCard>() // not using Discard Pile because those cards are from previous rounds
			.Where(card=>card.Cost <= maxCost)
			.Where(card=>ctx.Self.IsActiveDuring(ctx.GameState.Phase,card)) 
			.ToArray(); 
		if(options.Length == 0) return;

		PowerCard factory = await ctx.Self.SelectPowerCard( "Select card to replay", options, CardUse.Repeat, Present.Always );
		if(factory == null) return;

		ctx.Self.AddActionFactory( factory );

	}

	readonly int maxCost;
}
