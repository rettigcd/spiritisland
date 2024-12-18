namespace SpiritIsland.JaggedEarth;

public class ExaltationOfMoltenStone {

	[SpiritCard("Exaltation of Molten Stone",1, Element.Moon,Element.Fire,Element.Earth), Fast, AnotherSpirit]
	[Instructions( "Split 1 Energy per Fire you have between yourself and target Spirit, as evenly as possible. Target Spirit gains +1 Range with their powers that originate from a Mountain." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync(TargetSpiritCtx ctx ) {
		// Split 1 Energy per fire you have between yourself and target Spirit, as evenly as possible.
		int fireCount = await ctx.Self.Elements.CommitToCount(Element.Fire);
		int energyForSelf = fireCount / 2; // will round down
		int energyForOther = fireCount - energyForSelf;
		ctx.Self.Energy += energyForSelf;
		ctx.Other.Energy += energyForOther;

		// Target Spirit gains +1 range with their Powers that originate from a Mountain
		ExtendRangeFromMountains( ctx.Other );
	}

	static void ExtendRangeFromMountains( Spirit self ) {
		self.PowerRangeCalc = new ExtendRange1FromMountain( self.PowerRangeCalc );
	}

	class ExtendRange1FromMountain( ICalcRange previous ) : DefaultRangeCalculator(previous) {
		readonly TerrainMapper _powerTerrainMapper = ActionScope.Current.TerrainMapper;

		public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc) {
			if( _powerTerrainMapper.MatchesTerrain(source, Terrain.Mountain) )
				tc = tc.ExtendRange(1);
			return Previous!.GetTargetingRoute(source, tc);
		}

	}

}