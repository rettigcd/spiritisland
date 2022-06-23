namespace SpiritIsland.PromoPack1;

class PourDownPower 
//	: IActionFactory
{

	static public SpecialRule Rule => new SpecialRule(
		"Pour Down Power Across the Island",
		"For each 2 water you have, during the Fast/Slow phase you may either: Gain 1 Energy; or Repeat a land-targeting Power Card by paying its cost again. (Max 5)"
	);

	#region constructor

	public PourDownPower( DownpourDrenchesTheWorld spirit ) {
		this.spirit = spirit;
	}

	#endregion

	#region PowerCard Props

//	public string Name => "Pour Down Power Across the Island";

//	string IOption.Text => $"{Name} ({Remaining})";

//	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => speed == Phase.Fast || speed == Phase.Slow;

	//public Task ActivateAsync( SelfCtx ctx ) {
	//	++usedWaterActions;
	//	return Cmd.Pick1(
	//		// Gain Energy
	//		new ActionOption<SelfCtx>( "Gain 1 energy", x => x.Self.Energy++ ),
	//		// Repeat Phase Card that is already played for cost.
	//		new ActionOption<SelfCtx>( "Repeat card for Cost", x => new RepeatLandCardForCost().ActivateAsync( x ) )
	//	).Execute( ctx );
	//}

	#endregion

	#region Manager...

	public IEnumerable<IActionFactory> GetAvailableActions( Phase speed ) {

		if((speed == Phase.Fast || speed == Phase.Slow) && Remaining > 0) {
			yield return gainEnergy;
			if(repeatCard.GetCardOptions(this.spirit,speed).Any())
				yield return repeatCard;
		}
	}
	public bool RemoveFromUnresolvedActions( IActionFactory selectedActionFactory ) {
		if( selectedActionFactory != gainEnergy && selectedActionFactory != repeatCard)
			return false;
		++this.usedWaterActions;
		return true;
	}

	public void Reset() { usedWaterActions = 0; }

	public int Remaining => spirit.Elements[Element.Water] / 2 - usedWaterActions;
	readonly DownpourDrenchesTheWorld spirit;
	int usedWaterActions = 0;
	readonly PourDownPowerGainEnergy gainEnergy = new PourDownPowerGainEnergy();
	readonly RepeatLandCardForCost repeatCard = new RepeatLandCardForCost();

	#endregion
}

public class RepeatLandCardForCost : RepeatCardForCost {
	public override string Name => $"Repeat Land Card (PDP)";
	public RepeatLandCardForCost( params string[] exclude ) : base( exclude ) { }
	public override PowerCard[] GetCardOptions( Spirit self, Phase phase ) {
		return base.GetCardOptions( self,phase )
			.Where( card => card.LandOrSpirit == LandOrSpirit.Land )
			.ToArray();
	}
}

class PourDownPowerGainEnergy : IActionFactory {

	#region PowerCard Props

	public string Name => "Gain 1 Energy (PDP)";

	public string Text => Name;

	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => speed == Phase.Fast || speed == Phase.Slow;

	public Task ActivateAsync( SelfCtx ctx ) {
		++ctx.Self.Energy;
		return Task.CompletedTask;
	}

	#endregion

}


