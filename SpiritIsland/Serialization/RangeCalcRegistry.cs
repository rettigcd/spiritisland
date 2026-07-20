namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves an ICalcRange by tag - same tag-dispatch shape as BlightCardRegistry/
/// TargetingSourceStrategyRegistry. Closes the "known gap" flagged in
/// docs/GameSerialization-Roadmap.md section 2: an *active* temporary override (any ICalcRange with a
/// non-null Previous - RangeExtender, IncludeALandRangeCalculator, SkyStretchesToShoreApi,
/// ExtendRange1FromMountain) used to throw in Spirit.ToJson/RestoreFromJson. Only 5 implementers exist
/// solution-wide (including the default), so a small closed registry (not a general Spirit.Mods
/// mechanism) is enough.
/// </summary>
public static class RangeCalcRegistry {

	public static ICalcRange Deserialize( JsonArray json, ISerializationContext ctx ) {
		string tag = json[0]!.GetValue<string>();
		return _readers[tag]( json, ctx );
	}

	public static void Register( string tag, Func<JsonArray, ISerializationContext, ICalcRange> reader ) => _readers[tag] = reader;

	static readonly Dictionary<string, Func<JsonArray, ISerializationContext, ICalcRange>> _readers = [];

}
