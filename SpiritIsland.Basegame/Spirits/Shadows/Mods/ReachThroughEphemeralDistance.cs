
namespace SpiritIsland.Basegame;

class ReachThroughEphemeralDistance(Spirit spirit) : DefaultRangeCalculator, ISpiritMod, ICleanupSpiritWhenTimePasses {

	public const string Name = "Reach Through Ephemeral Distance";
	const string Description = "Once per turn, you may ignore Range. (* Currently, this only works for targetting.)";
	// This can be during Growth or for a Power - anything for which there's a Range arrow or the word "Range" is used.
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	static public void InitAspect(Spirit spirit) {
		var reach = new ReachThroughEphemeralDistance(spirit);
		spirit.PowerRangeCalc = reach;
		spirit.NonPowerRangeCalc = reach;
		spirit.SelectionMade += reach.Spirit_SelectionMade;
		// Not otherwise in spirit.Mods (dispatched via PowerRangeCalc/SelectionMade instead)
		// - added here solely so ICleanupSpiritWhenTimePasses resets _usedThisRound each round.
		spirit.Mods.Add(reach);
	}

	#region ICalcRange

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria targetCriteria) {
		var routes = DefaultRangeCalculator.Singleton.GetTargetingRoute(source, targetCriteria);

		var gs = GameState.Current;
		if( !_usedThisRound ) {
			var moreTargets = gs.Spaces.Except(routes.Targets).ToArray();
			_bonusTargets.AddRange(moreTargets);
			routes.AddRoutes(routes.Sources.SelectMany(s => moreTargets.Select(t => new TargetRoute(s, t))));
		}
		return routes;
	}

	#endregion ICalcRange

	#region Spirit_SelectionMade

	void Spirit_SelectionMade(object? obj) {
		if(obj is Space space )
			CheckAndClear(space);
		else if (obj is SpaceToken spaceToken)
			CheckAndClear(spaceToken.Space);
		else if (obj is Move move ) {
			if( move.From is Space fromSpace )
				CheckAndClear(fromSpace);
			if( move.Destination is Space toSpace )
				CheckAndClear(toSpace);
		}
	}

	void CheckAndClear(Space space1,Space? space2=null) {

		if( _bonusTargets.Contains(space1) || space2 is not null && _bonusTargets.Contains(space2) )
			_usedThisRound = true;

		// Once they pick a space, clear the bonuses so they don't get charged
		_bonusTargets.Clear();
	}

	#endregion Spirit_SelectionMade

	#region private fields

	bool _usedThisRound;

	// Only ever non-empty mid-decision, between GetTargetingRoute populating it and Spirit_SelectionMade
	// clearing it - always empty at any valid save boundary (no ActionScope awaiting input), so it isn't
	// captured - same "action boundary" scope as the rest of this project's save/restore design.
	readonly List<Space> _bonusTargets = [];

	void ICleanupSpiritWhenTimePasses.CleanupSpirit( Spirit spirit ) => _usedThisRound = false;

	#endregion private fields


}
