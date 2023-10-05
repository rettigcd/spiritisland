namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Allows Starlights Revealed Growth Options to be selected and available immediately
/// </summary>
class PlacePresenceAndRunAction : PlacePresence {
	public PlacePresenceAndRunAction(int range):base(range) { }
	public override Task ActivateAsync( SelfCtx ctx )
		=> Cmd.PlacePresenceWithin( new TargetCriteria( Range ), false ).Execute(ctx);
}