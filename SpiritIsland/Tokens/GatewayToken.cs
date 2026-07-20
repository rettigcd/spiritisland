namespace SpiritIsland;

/// <summary> Since tokens are directional, create separate 1 for each end. </summary>
/// <remarks> Must be in Engine project so that Memento can save/restore it.</remarks>
public class GatewayToken : ISpaceEntity, IHandleTokenRemoved, ISerializableSpaceEntity {

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

	// Resolved via spirit.Presence.Token rather than nesting _presence's own ToJson - a
	// SpiritPresenceToken is 1:1 with its owning spirit, so this avoids reconstructing a
	// second, different instance than the one the spirit itself already tracks.
	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx )
		=> new JsonArray( Tag, ctx.IndexOf( _presence.Self ), _from.Label, _to.Label );

	const string Tag = "GatewayToken";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => {
			Spirit spirit = ctx.SpiritAt( (int)json[1]! );
			SpaceSpec from = ctx.SpaceSpecByLabel( json[2]!.GetValue<string>() );
			SpaceSpec to = ctx.SpaceSpecByLabel( json[3]!.GetValue<string>() );

			// GatewayToken sits on both `from` and `to` in Tokens_ForIsland's own per-space JSON, so
			// Tokens_ForIsland.FromJson calls this reader once per space - twice total for the one
			// logical token. Reuse whichever instance the first call already placed on `from` (its
			// constructor Init's both ends immediately) rather than constructing a second, disconnected
			// one that would re-Init both ends again and leave 2 separate instances sitting there.
			GatewayToken? existing = ctx.Tokens[from].OfType<GatewayToken>().FirstOrDefault();
			return existing ?? new GatewayToken( spirit.Presence.Token, from.ScopeSpace, to.ScopeSpace );
		} );
}
