
namespace SpiritIsland.Basegame;

class ReachThroughEphemeralDistance : DefaultRangeCalculator {

	public const string Name = "Reach Through Ephemeral Distance";
	const string Description = "Once per turn, you may ignore Range. (* Currently, this only works for targetting.)"; 
	// This can be during Growth or for a Power - anything for which there's a Range arrow or the word "Range" is used.
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	static public void InitAspect(Spirit spirit) { 
		var reach = new ReachThroughEphemeralDistance();
		spirit.PowerRangeCalc = reach;
		spirit.NonPowerRangeCalc = reach;
		spirit.SelectionMade += reach.Spirit_SelectionMade;
	}

	#region ICalcRange

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria targetCriteria) {
		var routes = DefaultRangeCalculator.Singleton.GetTargetingRoute(source, targetCriteria);

		var gs = GameState.Current;
		if( ShouldAddBonusSpaces(gs) ) {
			var moreTargets = gs.Spaces.Except(routes.Targets).ToArray();
			_bonusTargets.AddRange(moreTargets);
			routes.AddRoutes(routes.Sources.SelectMany(s => moreTargets.Select(t => new TargetRoute(s, t))));
		}
		return routes;
	}

	#endregion ICalcRange

	#region Spirit_SelectionMade

	void Spirit_SelectionMade(object obj) {
		if(obj is Space space )
			CheckAndClear(space);
		else if (obj is SpaceToken spaceToken)
			CheckAndClear(spaceToken.Space);
		else if (obj is Move move )
			CheckAndClear(move.Source.Space,move.Destination);
	}

	void CheckAndClear(Space space1,Space? space2=null) {

		if( _bonusTargets.Contains(space1) || space2 is not null && _bonusTargets.Contains(space2) )
			DisableBonusSpaces();

		// Once they pick a space, clear the bonuses so they don't get charged
		_bonusTargets.Clear();
	}

	#endregion Spirit_SelectionMade

	#region private fields

	static bool ShouldAddBonusSpaces(GameState gs) => !gs.RoundScope.ContainsKey(Name);
	static void DisableBonusSpaces() { GameState.Current.RoundScope[Name] = true; }

	readonly List<Space> _bonusTargets = [];

	#endregion private fields

}