namespace SpiritIsland;

/// <summary> Since tokens are directional, create separate 1 for each end. </summary>
/// <remarks> Must be in Engine project so that Memento can save/restore it.</remarks>
public class GatewayToken : ISpaceEntity, IHandleTokenRemoved {

	readonly SpaceState _from;
	readonly SpaceState _to;

	public GatewayToken( SpiritPresenceToken presence, SpaceState from, SpaceState to ) {
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
	public IEntityClass Class => Token.OpenTheWays;

	public SpaceState GetLinked( SpaceState end ) => end == _from ? _to : end == _to ? _from : null; // doesn't link.
	public void HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(args.Removed == _presence && args.From[_presence] < 2)
			ActionScope.Current.AtEndOfThisAction( _ => RemoveSelf() );
	}

	readonly SpiritPresenceToken _presence;
}
