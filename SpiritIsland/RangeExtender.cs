namespace SpiritIsland;

public class RangeExtender( int extension, ICalcRange originalApi ) : DefaultRangeCalculator(originalApi) {

	static public void Extend( Spirit spirit, int extension ) {
		spirit.PowerRangeCalc = new RangeExtender( extension, spirit.PowerRangeCalc );
	}

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc)
		=> Previous!.GetTargetingRoute(source, tc.ExtendRange(_extension));

	public override JsonArray ToJson( ISerializationContext ctx ) => new JsonArray( Tag, _extension, Previous!.ToJson( ctx ) );

	const string Tag = "RangeExtender";

	[ModuleInitializer]
	internal static new void RegisterSerialization()
		=> RangeCalcRegistry.Register( Tag, ( json, ctx ) => new RangeExtender(
			json[1]!.GetValue<int>(), RangeCalcRegistry.Deserialize( (JsonArray)json[2]!, ctx )
		) );

	#region private
	readonly int _extension = extension;
	#endregion
}