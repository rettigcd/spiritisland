namespace SpiritIsland;

/// <summary> Stops space from being a source of Explorers</summary>
sealed public class SkipExploreFrom : BaseModEntity, IEndWhenTimePasses, ISkipExploreFrom, ISerializableSpaceEntity {

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

	const string Tag = "SkipExploreFrom";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new SkipExploreFrom() );
}
