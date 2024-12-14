namespace SpiritIsland.Basegame;

class ShadowsPartakeOfAmorphousSpace : IModifyAvailableActions {

	public const string Name = "Shadows Partake of Amorphous Space";
	const string Description = "During each Fast and each Slow phase, you may move 1 of your Presence to an adjacent land, or to a land with Dahan anywhere on the island.";
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	static public void InitAspect(Spirit spirit) => spirit.Mods.Add(new ShadowsPartakeOfAmorphousSpace(spirit));

	#region constructor / init

	public ShadowsPartakeOfAmorphousSpace(Spirit spirit) {
		_spirit = spirit;

		SpiritAction pushPresence = new SpiritAction("Push Presence", MovePresence);
		_moveFast = new SpiritGrowthAction(pushPresence, Phase.Fast);
		_moveSlow = new SpiritGrowthAction(pushPresence, Phase.Slow);
	}

	static async Task MovePresence(Spirit self) {
		var presenceToMove = await self.SelectAsync( new A.SpaceTokenDecision("Select Presence to move.",self.Presence.Deployed,Present.Done) );
		if(presenceToMove is null) return;

		var d2 = presenceToMove.Space.Adjacent.Union( GameState.Current.Spaces.Where(s => s.Dahan.Any) );
		Space destination = await self.SelectAsync(new A.SpaceDecision("Select destination for presence.", d2, Present.Always) );
		if(destination is not null)
			await presenceToMove.MoveTo(destination);
	}

	#endregion constructor / init

	#region IModifyAvailableActions imp

	void IModifyAvailableActions.Modify(List<IActionFactory> orig, Phase phase) {
		switch( phase ) {
			case Phase.Fast:
				AddUpTo(orig, _moveFast, 1);
				break;
			case Phase.Slow:
				AddUpTo(orig, _moveSlow, 1);
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
	readonly IActionFactory _moveFast;
	readonly IActionFactory _moveSlow;
	readonly Spirit _spirit;

	#endregion private fields

}
