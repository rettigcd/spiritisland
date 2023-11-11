namespace SpiritIsland.JaggedEarth;

public class ExaltationOfMoltenStone {

	[SpiritCard("Exaltation of Molten Stone",1, Element.Moon,Element.Fire,Element.Earth), Fast, AnotherSpirit]
	[Instructions( "Split 1 Energy per Fire you have between yourself and target Spirit, as evenly as possible. Target Spirit gains +1 Range with their powers that originate from a Mountain." ), Artist( Artists.MoroRogers )]
	public static Task ActAsync(TargetSpiritCtx ctx ) {
		// Split 1 Energy per fire you have between yourself and target Spirit, as evenly as possible.
		int fireCount = ctx.Self.Elements[Element.Fire];
		int energyForSelf = fireCount / 2; // will round down
		int energyForOther = fireCount - energyForSelf;
		ctx.Self.Energy += energyForSelf;
		ctx.Other.Energy += energyForOther;

		// Target Spirit gains +1 range with their Powers that originate from a Mountain
		ExtendRangeFromMountains( ctx.OtherCtx );

		return Task.CompletedTask;
	}

	static void ExtendRangeFromMountains( SelfCtx ctx ) {
		RangeCalcRestorer.Save(ctx.Self);
		ctx.Self.PowerRangeCalc = new ExtendRange1FromMountain( ctx.Self.PowerRangeCalc );
	}

	class ExtendRange1FromMountain : DefaultRangeCalculator {

		readonly ICalcRange _originalApi;
		readonly TerrainMapper _powerTerrainMapper;

		public ExtendRange1FromMountain( ICalcRange originalApi ) {
			_originalApi = originalApi;
			_powerTerrainMapper = ActionScope.Current.TerrainMapper;
		}

		public override IEnumerable<SpaceState> GetSpaceOptions( 
			SpaceState source, 
			TargetCriteria tc
		) {
			if(_powerTerrainMapper.MatchesTerrain( source, Terrain.Mountain ))
				tc = tc.ExtendRange(1);
			return _originalApi.GetSpaceOptions( source, tc );
		}

	}

}