namespace SpiritIsland.NatureIncarnate;

class RelentlessPunishment( Spirit spirit ) : IHandleActivatedActions {

	static public SpecialRule Rule => new SpecialRule(
		"Relentless Punishment",
		"If you had at least 3 Presence in the origin land, you may Repeat a Power Card any number of times on the same target land(s) by paying its cost +1/previous use."
	);

	void IHandleActivatedActions.ActionActivated(IActionFactory factory) {
		if( factory is not PowerCard powerCard ) return;
		var details = TargetSpaceAttribute.TargettedSpace;
		if( details is not null && details.sources.Any(s => spirit.Presence.CountOn(s) >= 0) )
			spirit.AddActionFactory(new RelentlessRepeater(powerCard, details.space));
	}
}