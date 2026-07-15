namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Intensify Through Understanding - Air/Water half. Lives in the Spirit's Mod bucket because
/// IModifyAvailableActions/IHandleActivatedActions/IConfigureMyActions are only dispatched via
/// Spirit.Mods.OfType&lt;T&gt;(), not the Island Mods token dictionary. See IntensifyThroughUnderstanding
/// for the other 6 elements (Moon/Sun/Fire/Plant/Animal/Earth), which live there instead - this
/// class's Initialize() constructs and registers that one separately once the game exists.
/// Not ISpaceEntity - Spirit.Mods has no restoration mechanism today (same as MarkedBeastMover).
/// </summary>
class IntensifyAirWater(ShiftingMemoryOfAges smoa)
	: IModifyAvailableActions					// Air
	, IHandleActivatedActions					// Air
	, IMoverFactory								// Water
	, IConfigureMyActions
	, IInitializeSpirit
{

	void IInitializeSpirit.Initialize() {
		GameState.Current.AddIslandMod(new IntensifyThroughUnderstanding(smoa)); // Moon/Sun/Fire/Plant/Animal/Earth
	}
	void IConfigureMyActions.Configure(Spirit spirit, ActionScope actionScope) {
		actionScope.MoverFactory = this; // water
	}

	#region Water

	DestinationSelector IMoverFactory.PushDestinations => _default.PushDestinations;

	TokenMover IMoverFactory.Gather(Spirit self, Space space) {
		var mover = _default.Gather(self, space);
		mover.DoEndStuff += Mover_DoEndStuff;
		return mover;
	}

	TokenMover IMoverFactory.Pusher(Spirit self, SourceSelector sourceSelector, DestinationSelector? dest) {
		var mover = _default.Pusher(self, sourceSelector, dest);
		mover.DoEndStuff += Mover_DoEndStuff;
		return mover;
	}

	async Task Mover_DoEndStuff(bool ranOutOfOption, Move[] arg2, Spirit arg3) {
		if( !ranOutOfOption ) return;
		if( _spirit.PreparedElementMgr.PreparedElements[Element.Water] == 0 ) return;

		var options = arg2.Where(m => 0 < m.Source.Count).ToArray();
		if( options.Length == 0 ) return;

		var move = await _spirit.Select("Boost 1 more move with Water element?", options, Present.Done);
		if( move is null ) return;
		_spirit.PreparedElementMgr.PreparedElements[Element.Water]--;
		await move.Apply();
	}
	readonly DefaultMoverFactory _default = new();

	#endregion Water

	#region Air
	// The rule is "max 1 of each Marker per Action" - for every other element that's enforced by
	// scoping the check to the current action (see DoBoost/IsUsed/MarkUsed in
	// IntensifyThroughUnderstanding). Air can't use that same mechanism because it doesn't modify an
	// action already in progress - it decides whether a slow power gets added to the list of what's
	// playable, before any action is chosen. But that still maps to "once per Action" automatically:
	// each Air spent corresponds to exactly one card being made playable and then played as one
	// action, so there's no way to spend Air twice on the same action. No separate usage counter is
	// needed - PreparedElements[Air] decrementing on use is the only cap required, and it naturally
	// allows spending another Air on a genuinely separate action later in the same round (e.g. an
	// extra action from another card), matching the card text.
	void IModifyAvailableActions.Modify(List<IActionFactory> orig, Phase phase) {
		bool canMakeSlowFast = phase == Phase.Fast && 0 < _spirit.PreparedElementMgr.PreparedElements[Element.Air];
		if( !canMakeSlowFast ) return;

		_slowAsFast = _spirit.AllActions.Where(slowAction => slowAction.CouldActivateDuring(Phase.Slow, _spirit))
			.Except(orig) // in case another mod added slow-as-fast, don't re-add it.
			.ToArray();
		orig.AddRange(_slowAsFast);
	}

	void IHandleActivatedActions.ActionActivated(IActionFactory factory) {
		if( _slowAsFast.Contains(factory) )
			--_spirit.PreparedElementMgr.PreparedElements[Element.Air];
	}

	IActionFactory[] _slowAsFast = [];
	#endregion Air

	readonly ShiftingMemoryOfAges _spirit = smoa;

}
