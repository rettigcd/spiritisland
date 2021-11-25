﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

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

		private static void ExtendRangeFromMountains( SpiritGameStateCtx ctx ) {
			ctx.GameState.TimePasses_ThisRound.Push( new PowerApiRestorer( ctx.Self ).Restore );
			ctx.Self.RangeCalc = new ExtendRange1FromMountain( ctx.Self.RangeCalc );
		}

		class ExtendRange1FromMountain : DefaultCalcRange {

			readonly ICalcRange originalApi;

			public ExtendRange1FromMountain( ICalcRange originalApi ) {
				this.originalApi = originalApi;
			}

			public override IEnumerable<Space> GetTargetOptionsFromKnownSource( Spirit self, GameState gameState, int range, string filterEnum, TargettingFrom powerType, IEnumerable<Space> source ) {
				// original options
				List<Space> spaces = originalApi.GetTargetOptionsFromKnownSource( self, gameState, range, filterEnum, powerType, source ).ToList();

				// Target Spirit gains +1 range with their Powers that originate from a Mountain
				var mountainSource = source.Where(x=>x.Terrain == Terrain.Mountain).ToArray();
				return mountainSource.Length == 0 ? spaces
					: spaces
					.Union( originalApi.GetTargetOptionsFromKnownSource( self, gameState, range+1, filterEnum, powerType, mountainSource ) )
					.Distinct();
			}

		}


	}


}
