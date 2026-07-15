namespace SpiritIsland;

/// <summary> Since tokens are directional, create separate 1 for each end. </summary>
/// <remarks> Must be in Engine project so that Memento can save/restore it.</remarks>
public class GatewayToken : ISpaceEntity, IHandleTokenRemoved {

	readonly SpaceSpec _from;
	readonly SpaceSpec _to;

	public GatewayToken( SpiritPresenceToken presence, Space from, Space to ) {
		_presence = presence;
		_from = from.SpaceSpec;
		_to = to.SpaceSpec;
		// Add self
		from.Init( this, 1 );
		to.Init( this, 1 );
	}
	public void RemoveSelf() {
		var scope = ActionScope.Current;
		scope.AccessTokens(_from).Init( this, 0 );
		scope.AccessTokens(_to).Init( this, 0 );
	}

	public Space? GetLinked( Space end ) {
		var scope = ActionScope.Current;
		return end.SpaceSpec == _from ? scope.AccessTokens(_to)
			: end.SpaceSpec == _to ? scope.AccessTokens(_from)
			: null; // doesn't link.
	}

	public Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
		var from = (Space)args.From;
		if(args.Removed == _presence && from[_presence] < 2)
			ActionScope.Current.AtEndOfThisAction( _ => RemoveSelf() );
		return Task.CompletedTask;
	}

	readonly SpiritPresenceToken _presence;
}
