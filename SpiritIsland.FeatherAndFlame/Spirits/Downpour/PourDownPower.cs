namespace SpiritIsland.FeatherAndFlame;

class PourDownPower : IModifyAvailableActions, IOwnedActionFactories {

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
	int TotalCount => _spirit.Elements[Element.Water] / 2;

	readonly RepeatLandCardForCost _a1 = new RepeatLandCardForCost();
	readonly PourDownPowerGainEnergy _a2 = new PourDownPowerGainEnergy();
	readonly Spirit _spirit;

	#endregion private

	#region Json

	// _a1/_a2 are compared by reference against spirit.UsedActions - restoring them must resolve back
	// to these exact instances (already re-added by this spirit's own constructor before
	// RestoreFromJson runs), not fresh ones built from their own serialized content. See
	// IOwnedActionFactories.
	const string Tag = "PourDownPower";

	string IOwnedActionFactories.ModTag => Tag;

	string? IOwnedActionFactories.KeyFor( IActionFactory factory )
		=> factory == _a1 ? "a1" : factory == _a2 ? "a2" : null;

	IActionFactory IOwnedActionFactories.ResolveActionFactory( string key ) => key switch {
		"a1" => _a1,
		"a2" => _a2,
		_ => throw new ArgumentException( $"Unknown key '{key}'" )
	};

	#endregion Json

	#region Actions

	class RepeatLandCardForCost(string exclude = "") : RepeatCardForCost(exclude) {
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