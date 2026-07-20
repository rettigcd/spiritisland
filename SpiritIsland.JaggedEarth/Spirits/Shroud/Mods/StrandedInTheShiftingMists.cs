
namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds the 2 special actions to the spirits available action list.
/// </summary>
class StrandedInTheShiftingMists(Spirit spirit) : IModifyAvailableActions, IOwnedActionFactories {

	static public SpecialRule Rule => new SpecialRule(
		"Stranded in the Shifting Mist",
		"Once each Fast phase, Isolate one of your lands."
	);

	#region constructor / init

	#endregion constructor / init

	#region IModifyAvailableActions imp

	void IModifyAvailableActions.Modify(List<IActionFactory> orig, Phase phase) {
		if(phase == Phase.Fast)
			if( !spirit.UsedActions.Any(x => x == _isolate) )
				orig.Add(_isolate);
	}

	#endregion IModifyAvailableActions imp

	#region Json

	// _isolate is compared by reference against spirit.UsedActions - restoring it must resolve back to
	// this exact instance (already re-added by the Stranded aspect's replay before RestoreFromJson
	// runs), not a fresh one built from its own serialized content. See IOwnedActionFactories.
	const string Tag = "StrandedInTheShiftingMists";

	string IOwnedActionFactories.ModTag => Tag;

	string? IOwnedActionFactories.KeyFor( IActionFactory factory ) => factory == _isolate ? "isolate" : null;

	IActionFactory IOwnedActionFactories.ResolveActionFactory( string key )
		=> key == "isolate" ? _isolate : throw new ArgumentException( $"Unknown key '{key}'" );

	#endregion Json

	#region private fields

	// We can do better than this.
	readonly IActionFactory _isolate = new GrowthAction(Cmd.Isolate.On().SpiritPickedLand().Which(Has.YourPresence), Phase.Fast);

	#endregion private fields

}

