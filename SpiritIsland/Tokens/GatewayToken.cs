namespace SpiritIsland;

/// <summary> Since tokens are directional, create separate 1 for each end. </summary>
/// <remarks> Must be in Engine project so that Memento can save/restore it.</remarks>
public class GatewayToken : ISpaceEntity, IHandleTokenRemoved {

	readonly Space _from;
	readonly Space _to;

	public GatewayToken( SpiritPresenceToken presence, Space from, Space to ) {
		_presence = presence;
		_from = from;
		_to = to;
		// Add self
		_from.Init( this, 1 );
		_to.Init( this, 1 );
	}
	public void RemoveSelf() {
		_from.Init( this, 0 );
		_to.Init( this, 0 );
	}

	public Space GetLinked( Space end ) => end == _from ? _to : end == _to ? _from : null; // doesn't link.
	public Task HandleTokenRemovedAsync( Space from, ITokenRemovedArgs args ) {
		if(args.Removed == _presence && from[_presence] < 2)
			ActionScope.Current.AtEndOfThisAction( _ => RemoveSelf() );
		return Task.CompletedTask;
	}

	readonly SpiritPresenceToken _presence;
}
