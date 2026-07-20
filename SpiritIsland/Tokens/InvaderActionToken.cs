namespace SpiritIsland;

/// <summary>
/// Generic implementation class used for marking Explores, Builds, and Ravages.
/// </summary>
public class InvaderActionToken( string label ) : ISpaceEntity, ISerializableSpaceEntity {
	public string Label { get; } = label;


	// Fake Tokens that are not visible.
	static readonly public InvaderActionToken DoExplore = new InvaderActionToken("Explore");
	static readonly public InvaderActionToken DoBuild = new InvaderActionToken("Build");
	static readonly public InvaderActionToken DoRavage = new InvaderActionToken("Ravage");

	// Same identity caveat as LandDamage/AJoiningOfSwarmsAndFlocks: DoExplore/DoBuild/DoRavage are
	// stable singletons used as dictionary keys throughout the codebase; FromJson reconstructs a
	// fresh instance rather than resolving back to one of the 3 singletons above. Fine for an
	// isolated round-trip, not yet safe for a full board restore.
	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, Label );

	const string Tag = "InvaderActionToken";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new InvaderActionToken( json[1]!.GetValue<string>() ) );

}