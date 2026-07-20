namespace SpiritIsland.Basegame;

class IncludeALandRangeCalculator(Spirit spirit, ICalcRange previous, Space target) : DefaultRangeCalculator(previous) {

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc) {
		var routes = Previous!.GetTargetingRoute(source, tc);
		routes.AddRoutes(RoutesToTarget(tc));
		return routes;
	}

	IEnumerable<TargetRoute> RoutesToTarget(TargetCriteria tc) => spirit.Presence.Lands
		.Where(tc.Matches)
		.Select(s => new TargetRoute(s, target));

	public override JsonArray ToJson( ISerializationContext ctx ) => new JsonArray(
		Tag, ctx.IndexOf( spirit ), Previous!.ToJson( ctx ), target.SpaceSpec.Label
	);

	const string Tag = "IncludeALandRangeCalculator";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> RangeCalcRegistry.Register( Tag, ( json, ctx ) => new IncludeALandRangeCalculator(
			ctx.SpiritAt( json[1]!.GetValue<int>() ),
			RangeCalcRegistry.Deserialize( (JsonArray)json[2]!, ctx ),
			ctx.Tokens[ ctx.SpaceSpecByLabel( json[3]!.GetValue<string>() ) ]
		) );

}