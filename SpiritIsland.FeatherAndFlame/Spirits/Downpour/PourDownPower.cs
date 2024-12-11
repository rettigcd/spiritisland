namespace SpiritIsland.FeatherAndFlame;

class PourDownPower : IModifyAvailableActions {

	static public SpecialRule Rule => new SpecialRule(
		"Pour Down Power Across the Island",
		"For each 2 water you have, during the Fast/Slow phase you may either: Gain 1 Energy; or Repeat a land-targeting Power Card by paying its cost again. (Max 5)"
	);

	public int Remaining => TotalCount - UsedCounts;

	#region constructor

	public PourDownPower(Spirit spirit) {
		_spirit = spirit;
	}

	#endregion constructor

	public void Modify(List<IActionFactory> orig, Phase phase) {
		int remaining = Remaining;
		if( 0 < remaining ) {
			orig.Add(_a1);
			orig.Add(_a2);
		}
	}

	#region private

	int UsedCounts => _spirit.UsedActions.Count(x => x == _a1 || x == _a2);
	int TotalCount => _spirit.Elements.Get(Element.Water) / 2;

	readonly RepeatLandCardForCost _a1 = new RepeatLandCardForCost();
	readonly PourDownPowerGainEnergy _a2 = new PourDownPowerGainEnergy();
	readonly Spirit _spirit;

	#endregion private

	#region Actions

	class RepeatLandCardForCost(params string[] exclude) : RepeatCardForCost(exclude) {
		public override string Title => $"Repeat Land Card (PDP)";

		public override PowerCard[] GetCardOptions(Spirit self, Phase phase) {
			return base.GetCardOptions(self, phase)
				.Where(card => card.LandOrSpirit == LandOrSpirit.Land)
				.ToArray();
		}
	}

	class PourDownPowerGainEnergy : IActionFactory {

		#region PowerCard Props

		public string Title => "Gain 1 Energy (PDP)";

		public string Text => Title;

		public bool CouldActivateDuring(Phase speed, Spirit spirit) => speed == Phase.Fast || speed == Phase.Slow;

		public Task ActivateAsync(Spirit self) {
			++self.Energy;
			return Task.CompletedTask;
		}


		#endregion

	}

	#endregion Actions

}