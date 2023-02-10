namespace SpiritIsland;

/// <summary> Since tokens are directional, create separate 1 for each end. </summary>
/// <remarks> Must be in Engine project so that Memento can save/restore it.</remarks>
public class GatewayToken : ISpaceEntity, IHandleTokenRemoved {

	SpaceState From { get; }
	public SpaceState To { get; }

	public GatewayToken( SpiritPresenceToken presence, SpaceState from, SpaceState to ) {
		_presence = presence;
		From = from;
		To = to;
		// Add self
		From.Init( this, 1 );
		To.Init( this, 1 );
	}
	public void RemoveSelf() {
		From.Init( this, 0 );
		To.Init( this, 0 );
	}
	public IEntityClass Class => Token.OpenTheWays;

	public SpaceState GetLinked( SpaceState end ) => end == From ? To : end == To ? From : null; // doesn't link.
	public Task HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(args.Token == _presence && args.RemovedFrom[_presence] < 2)
			RemoveSelf(); // !!! maybe instead of removing now, remove at end of action

		return Task.CompletedTask;
	}

	readonly SpiritPresenceToken _presence;
}
