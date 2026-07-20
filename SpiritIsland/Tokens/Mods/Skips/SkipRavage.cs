namespace SpiritIsland;

/// <summary> Stops 1 Ravage. </summary>
public class SkipRavage(string label, UsageDuration duration = UsageDuration.SkipOneThisTurn)
	: BaseModEntity, IEndWhenTimePasses, ISkipRavages, ISerializableSpaceEntity {

	/// <summary> So we can log what it was that stopped the ravage. </summary>
	public string SourceLabel { get; } = label;

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public virtual Task<bool> Skip(Space space) {
		if( _duration == UsageDuration.SkipOneThisTurn )
			space.Adjust(this, -1); // remove this token
		return Task.FromResult(true); // stopped
	}

	#region private

	readonly UsageDuration _duration = duration;

	#endregion

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, SourceLabel, (int)_duration );

	const string Tag = "SkipRavage";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new SkipRavage( json[1]!.GetValue<string>(), (UsageDuration)json[2]!.GetValue<int>() ) );

}

