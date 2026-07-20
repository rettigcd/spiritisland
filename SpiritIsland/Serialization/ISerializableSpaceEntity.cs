using System.Text.Json.Nodes;

namespace SpiritIsland.Serialization;

/// <summary>
/// Implemented by `ISpaceEntity` types that support JSON serialization.
/// The first element of the returned array is always the type-tag used to find the matching
/// reader in <see cref="SpaceEntitySerialization"/> on the way back in.
/// </summary>
public interface ISerializableSpaceEntity {
	JsonArray ToJson( ISerializationContext ctx );
}
