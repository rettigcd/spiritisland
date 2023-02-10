namespace SpiritIsland.JaggedEarth;

public class ExaltationOfMoltenStone {

	[SpiritCard("Exaltation of Molten Stone",1, Element.Moon,Element.Fire,Element.Earth), Fast, AnotherSpirit]
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
		RangeCalcRestorer.Save(ctx.Self,ctx.GameState);
		ctx.Self.PowerRangeCalc = new ExtendRange1FromMountain( ctx.Self.PowerRangeCalc );
	}

	class ExtendRange1FromMountain : DefaultRangeCalculator {

		readonly ICalcRange _originalApi;
		readonly TerrainMapper _powerTerrainMapper;

		public ExtendRange1FromMountain( ICalcRange originalApi ) {
			_originalApi = originalApi;
			_powerTerrainMapper = ActionScope.Current.TerrainMapper;
		}

		public override IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( 
			IEnumerable<SpaceState> source, 
			TargetCriteria tc
		) {
			// original options
			var spaces = _originalApi.GetTargetOptionsFromKnownSource( source, tc ).ToList();

			// Target Spirit gains +1 range with their Powers that originate from a Mountain
			var mountainSource = source.Where(space => _powerTerrainMapper.MatchesTerrain( space, Terrain.Mountain) ).ToArray();
			return mountainSource.Length == 0 ? spaces
				: spaces
				.Union( _originalApi.GetTargetOptionsFromKnownSource( mountainSource, tc.ExtendRange(1) ) )
				.Distinct();
		}

	}

}