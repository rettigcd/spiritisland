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
		ctx.GameState.TimePasses_ThisRound.Push( new RangeCalcRestorer( ctx.Self ).Restore );
		ctx.Self.PowerRangeCalc = new ExtendRange1FromMountain( ctx.Self.PowerRangeCalc );
	}

	class ExtendRange1FromMountain : DefaultPowerRangeCalculator {

		readonly ICalcPowerRange originalApi;

		public ExtendRange1FromMountain( ICalcPowerRange originalApi ) {
			this.originalApi = originalApi;
		}

		public override IEnumerable<Space> GetTargetOptionsFromKnownSource( SelfCtx ctx, TargetingPowerType powerType, IEnumerable<SpaceState> source, TargetCriteria tc ) {
			// original options
			List<Space> spaces = originalApi.GetTargetOptionsFromKnownSource( ctx, powerType, source, tc ).ToList();

			// Target Spirit gains +1 range with their Powers that originate from a Mountain
			var mountainSource = source.Where(space => ctx.TerrainMapper.MatchesTerrain( space, Terrain.Mountain) ).ToArray();
			return mountainSource.Length == 0 ? spaces
				: spaces
				.Union( originalApi.GetTargetOptionsFromKnownSource( ctx, powerType, mountainSource, new TargetCriteria(tc.Range+1, tc.Filter) ) )
				.Distinct();
		}

	}

}