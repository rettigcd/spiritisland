namespace SpiritIsland.FeatherAndFlame;

class PourDownPower( DownpourDrenchesTheWorld _spirit ) : IRunWhenTimePasses {

	static public SpecialRule Rule => new SpecialRule(
		"Pour Down Power Across the Island",
		"For each 2 water you have, during the Fast/Slow phase you may either: Gain 1 Energy; or Repeat a land-targeting Power Card by paying its cost again. (Max 5)"
	);

	public int Remaining => _spirit.Elements.Get(Element.Water) / 2 - _usedWaterActions;

	public IEnumerable<IActionFactory> GetAvailableActions( Phase speed ) {

		if((speed == Phase.Fast || speed == Phase.Slow) && Remaining > 0) {
			yield return _gainEnergy;

			// if played card, can now repeat it.
			if(_repeatCard.GetCardOptions(_spirit,speed).Length != 0)
				yield return _repeatCard;
		}
	}

	public bool RemoveFromUnresolvedActions( IActionFactory selectedActionFactory ) {
		if( selectedActionFactory != _gainEnergy && selectedActionFactory != _repeatCard)
			return false;
		++this._usedWaterActions;
		return true;
	}

	public void Reset() { _usedWaterActions = 0; }

	#region IRunWhenTimePasses imp

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	bool IRunWhenTimePasses.RemoveAfterRun => false;

	Task IRunWhenTimePasses.TimePasses( GameState gameStTate ){
		Reset();
		return Task.CompletedTask;
	}

	#endregion IRunWhenTimePasses imp

	#region private fields

	int _usedWaterActions = 0;
	readonly PourDownPowerGainEnergy _gainEnergy = new PourDownPowerGainEnergy();
	readonly RepeatLandCardForCost _repeatCard = new RepeatLandCardForCost();

	#endregion
}

public class RepeatLandCardForCost( params string[] exclude ) : RepeatCardForCost( exclude ) {
	public override string Title => $"Repeat Land Card (PDP)";

	public override PowerCard[] GetCardOptions( Spirit self, Phase phase ) {
		return base.GetCardOptions( self,phase )
			.Where( card => card.LandOrSpirit == LandOrSpirit.Land )
			.ToArray();
	}
}

class PourDownPowerGainEnergy : IActionFactory {

	#region PowerCard Props

	public string Title => "Gain 1 Energy (PDP)";

	public string Text => Title;

	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => speed == Phase.Fast || speed == Phase.Slow;

	public Task ActivateAsync( Spirit self ) {
		++self.Energy;
		return Task.CompletedTask;
	}


	#endregion

}


