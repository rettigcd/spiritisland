using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class SkyStretchesToShore {

		[MinorCard( "Sky Stretches to Shore", 1, Speed.Fast, Element.Sun, Element.Air, Element.Water, Element.Earth )]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {

			// this turn, target spirit may use 1 slow power as if it wer fast or vice versa
			ctx.Target.AddActionFactory( new ChangeSpeed() );

			// Target Spirit gains +3 range for targeting costal lands only
			var original = ctx.Self.PowerApi;
			ctx.Self.PowerApi = new SkyStretchesToShoreApi(original);
			ctx.GameState.TimePasses_ThisRound.Push( RestoreApi );
			Task RestoreApi(GameState _ ) {
				ctx.Self.PowerApi = original;
				return Task.CompletedTask;
			}

			return Task.CompletedTask;
		}

	}

	class SkyStretchesToShoreApi : TargetLandApi {
		public SkyStretchesToShoreApi( TargetLandApi orig ) {
			this.orig = orig;
		}

		public override IEnumerable<Space> GetTargetOptions( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			var normal = orig.GetTargetOptions( self, gameState, sourceEnum, sourceTerrain, range, filterEnum );
			var shore = orig.GetTargetOptions( self, gameState, sourceEnum, sourceTerrain, range+3, filterEnum )
				.Where(x => x.IsCostal);
			return normal.Union(shore);
		}

		TargetLandApi orig;
	}

}
