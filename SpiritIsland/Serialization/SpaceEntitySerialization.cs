using System.Text.Json.Nodes;

namespace SpiritIsland.Serialization;

/// <summary>
/// Dispatch table from a type-tag (`json[0]`) to the reader that reconstructs that type.
/// Writing needs no registry - it's just <see cref="ISerializableSpaceEntity.ToJson"/>.
/// Types register their reader via a `[ModuleInitializer]` method so this project doesn't
/// need to know about every project that defines a serializable entity.
/// </summary>
public static class SpaceEntitySerialization {

	public static JsonArray Serialize( ISerializableSpaceEntity entity, ISerializationContext ctx ) => entity.ToJson( ctx );

	public static object Deserialize( JsonArray json, ISerializationContext ctx ) {
		string tag = json[0]!.GetValue<string>();
		return _readers[tag]( json, ctx );
	}

	public static void Register( string tag, Func<JsonArray, ISerializationContext, object> reader ) => _readers[tag] = reader;

	static readonly Dictionary<string, Func<JsonArray, ISerializationContext, object>> _readers = [];

}
