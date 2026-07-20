namespace SpiritIsland;

/// <summary> Stops either 1 or ALL explores </summary>
public class SkipExploreTo(bool skipAll = false) : BaseModEntity, IEndWhenTimePasses, ISkipExploreTo, ISerializableSpaceEntity {
	public virtual Task<bool> Skip(Space space) {
		if( !skipAll )
			space.Adjust(this, -1); // remove this token
		return Task.FromResult(true); // stopped
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, skipAll );

	const string Tag = "SkipExploreTo";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new SkipExploreTo( json[1]!.GetValue<bool>() ) );
}
