namespace SpiritIsland.JaggedEarth;

class MistsSteadilyDrift : IModifyAvailableActions {

	static public SpecialRule Rule => new SpecialRule(
		"Mists Steadily Drift",
		"Up to twice during the Fast phase and up to twice during the Slow phase, Push 1 of your Presence."
	);

	#region constructor / init

	public MistsSteadilyDrift(Spirit spirit) {
		_spirit = spirit;

		SpiritAction pushPresence = new SpiritAction("Push Presence", PushPresenceAsync);
		_pushFast = new GrowthAction(pushPresence, Phase.Fast);
		_pushSlow = new GrowthAction(pushPresence, Phase.Slow);
	}

	static async Task PushPresenceAsync(Spirit self) {
		var pushSpaces = self.Presence.Lands.ToArray();
		var token = await self.SelectAsync(new A.SpaceTokenDecision("Select presence to push", self.Presence.Deployed, Present.AutoSelectSingle));
		if(token is null) return;
		// #pushpresence
		Space? destination = await self.SelectAsync(A.SpaceDecision.ToPushPresence(token.Space, token.Space.Adjacent, Present.Always, token.Token));
		if(destination is null) return;
		// apply...
		await token.MoveTo(destination);
	}

	#endregion constructor / init

	#region IModifyAvailableActions imp

	void IModifyAvailableActions.Modify(List<IActionFactory> orig, Phase phase) {
		switch( phase ) {
			case Phase.Fast:
				AddUpTo(orig, _pushFast, 2);
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

	// 2 different copies so we can distinguish used-fast vs used-slow
	readonly IActionFactory _pushFast;
	readonly IActionFactory _pushSlow;
	readonly Spirit _spirit;

	#endregion private fields

}

