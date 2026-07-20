namespace SpiritIsland;

/// <summary> Base for dynamic Defend tokens - THIS ROUND ONLY. </summary>
public abstract class DefendToken(string badge) : DynamicToken(new TokenVariety(Token.Defend, badge)) {}

/// <summary> Defend equal to a spirit's presence count in the land. </summary>
public class PresenceCountDefend(SpiritPresence presence, string badge) : DefendToken(badge), ISerializableSpaceEntity {
	protected override int GetCount(Space space) => presence.CountOn(space);

	// Resolved via spirit.Presence rather than needing a way to construct/hold a SpiritPresence
	// standalone - a spirit always has exactly one, so an index is all that's needed.
	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx )
		=> new JsonArray( Tag, ctx.IndexOf( presence.Token.Self ), badge );

	const string Tag = "PresenceCountDefend";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx )
			=> new PresenceCountDefend( ctx.SpiritAt( (int)json[1]! ).Presence, json[2]!.GetValue<string>() ) );
}
