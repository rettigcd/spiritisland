
namespace SpiritIsland.JaggedEarth;

public class Stranded : IAspect {

	static public AspectConfigKey ConfigKey => new AspectConfigKey(ShroudOfSilentMist.Name, Name);

	public const string Name = "Stranded";

	public void ModSpirit(Spirit spirit) {
		var shroud = (ShroudOfSilentMist)spirit;
		shroud.EnableMistsShiftAndFlow = false;
		shroud.AvailableActionMods.Add( new StrandedActions(shroud) );
		shroud.SpecialRules = [..shroud.SpecialRules.Where(x=>x != MistsShiftAndFlow.Rule), MistsSteadilyDrift, StrandedInTheShiftingMists];
	}

	SpecialRule MistsSteadilyDrift => new SpecialRule(
		"Mists Steadily Drift",
		"Up to twice during the Fast phase and up to twice during the Slow phase, Push 1 of your Presence."
	);
	SpecialRule StrandedInTheShiftingMists => new SpecialRule(
		"Stranded in the Shifting Mist",
		"Once each Fast phase, Isolate one of your lands."
	);
}

class StrandedActions : IModifyAvailableActions {

	#region constructor / init

	public StrandedActions(Spirit spirit) {
		_spirit = spirit;

		SpiritAction pushPresence = new SpiritAction("Push Presence", PushPresenceAsync);
		_pushFast = new SpiritGrowthAction(pushPresence, Phase.Fast);
		_pushSlow = new SpiritGrowthAction(pushPresence, Phase.Slow);
	}

	static async Task PushPresenceAsync(Spirit self) {
		var pushSpaces = self.Presence.Lands.ToArray();
		var token = await self.SelectAsync(new A.SpaceTokenDecision("Select presence to push", self.Presence.Deployed, Present.AutoSelectSingle));
		// #pushpresence
		Space destination = await self.SelectAsync(A.SpaceDecision.ToPushPresence(token.Space, token.Space.Adjacent, Present.Always, token.Token));
		// apply...
		await token.MoveTo(destination);
	}

	#endregion constructor / init

	#region IModifyAvailableActions imp

	void IModifyAvailableActions.Modify(List<IActionFactory> orig, Phase phase) {
		switch( phase ) {
			case Phase.Fast:
				AddUpTo(orig, _pushFast, 2);
				AddUpTo(orig, _isolate, 1);
				break;
			case Phase.Slow:
				AddUpTo(orig, _pushSlow, 2);
				break;
		}
	}

	void AddUpTo(List<IActionFactory> orig, IActionFactory action, int max) {
		if( _spirit.UsedActions.Count(x => x == action) < max )
			orig.Add(action);
	}
	#endregion IModifyAvailableActions imp

	#region private fields

	// We can do better than this.
	readonly IActionFactory _pushFast;
	readonly IActionFactory _pushSlow;
	readonly IActionFactory _isolate = new SpiritGrowthAction( Cmd.Isolate.On().SpiritPickedLand().Which(Has.YourPresence), Phase.Fast );
	readonly Spirit _spirit;

	#endregion private fields

}
