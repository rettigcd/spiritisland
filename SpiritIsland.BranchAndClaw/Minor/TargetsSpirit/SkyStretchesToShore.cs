namespace SpiritIsland.BranchAndClaw;

public class SkyStretchesToShore {

	[MinorCard( "Sky Stretches to Shore", 1, Element.Sun, Element.Air, Element.Water, Element.Earth ),Fast,AnySpirit]
	[Instructions( "This turn, target Spirit may use 1 Slow Power as if it were Fast, or vice versa. Target Spirit gains +3 Range for targeting Coastal lands only." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync( TargetSpiritCtx ctx ) {

		// this turn, target spirit may use 1 slow power as if it were fast or vice versa
		ctx.Other.AddActionFactory( new ResolveSlowDuringFast_OrViseVersa() );

		// Target Spirit gains +3 range for targeting coastal lands only
		ctx.Other.PowerRangeCalc = new SkyStretchesToShoreApi( ctx.Other );

		return Task.CompletedTask;
	}

}

class SkyStretchesToShoreApi : DefaultRangeCalculator {

	public SkyStretchesToShoreApi( Spirit self ) : this( self, self.PowerRangeCalc ) { }

	// Used by FromJson to rebuild onto the exact recursively-resolved previous chain, rather than the
	// convenience constructor above, which would instead capture whatever self.PowerRangeCalc happens
	// to be *at restore time* (already-reconstructed spirit's post-setup value, not necessarily the
	// originally-saved previous if something else was also temporarily layered on top).
	SkyStretchesToShoreApi( Spirit self, ICalcRange previous ) : base( previous ) => _self = self;

	readonly Spirit _self;

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc) {
		var normal = Previous!.GetTargetingRoute(source, tc);
		var shore = Previous!.GetTargetingRoute(source, tc.ExtendRange(3))._routes
			.Where(x => x.target.SpaceSpec.IsCoastal);
		return new TargetRoutes( normal._routes.Union(shore) );
	}

	public override JsonArray ToJson( ISerializationContext ctx ) => new JsonArray( Tag, ctx.IndexOf( _self ), Previous!.ToJson( ctx ) );

	const string Tag = "SkyStretchesToShoreApi";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> RangeCalcRegistry.Register( Tag, ( json, ctx ) => new SkyStretchesToShoreApi(
			ctx.SpiritAt( json[1]!.GetValue<int>() ), RangeCalcRegistry.Deserialize( (JsonArray)json[2]!, ctx )
		) );

}