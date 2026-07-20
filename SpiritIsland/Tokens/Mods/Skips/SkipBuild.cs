namespace SpiritIsland;

/// <summary> Stops either 1 or ALL builds. </summary>
public class SkipBuild(string label, UsageDuration duration, params ITokenClass[] stoppedTokenClasses) : BaseModEntity, IEndWhenTimePasses, ISkipBuilds, ISerializableSpaceEntity {

	readonly ITokenClass[] _stoppedClasses = stoppedTokenClasses.Length > 0 ? stoppedTokenClasses : Human.Town_City;

	static public SkipBuild Default(string label) => new SkipBuild(label, UsageDuration.SkipOneThisTurn);

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	public string Text { get; } = label;

	bool Stops(ITokenClass buildClass) => _stoppedClasses.Contains(buildClass);

	public virtual Task<bool> Skip(Space space) {
		if( !Stops(BuildEngine.InvaderToAdd.Value!) ) return Task.FromResult(false); // not stopped

		if( duration == UsageDuration.SkipOneThisTurn )
			space.Adjust(this, -1); // remove this token

		return Task.FromResult(true); // stopped
	}

	#region Serialization

	[ModuleInitializer]
	internal static void RegisterSerialization() => SpaceEntitySerialization.Register( Tag, FromJson );

	const string Tag = "SkipBuild";

	JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx )
		=> new JsonArray( Tag, Text, (int)duration, new JsonArray( _stoppedClasses.Select( t => (JsonNode)t.Label ).ToArray() ) );

	static object FromJson( JsonArray json, ISerializationContext ctx ) {
		string label = json[1]!.GetValue<string>();
		UsageDuration duration = (UsageDuration)json[2]!.GetValue<int>();
		ITokenClass[] classes = json[3]!.AsArray().Select( n => ctx.TokenClassByLabel( n!.GetValue<string>() ) ).ToArray();
		return new SkipBuild( label, duration, classes );
	}

	#endregion

}