﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SkyStretchesToShore {

		[MinorCard( "Sky Stretches to Shore", 1, Element.Sun, Element.Air, Element.Water, Element.Earth )]
		[Fast]
		[AnySpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {

			// this turn, target spirit may use 1 slow power as if it wer fast or vice versa
			ctx.Other.AddActionFactory( new ChangeSpeed() );

			// Target Spirit gains +3 range for targeting costal lands only
			TargetLandApi.ScheduleRestore( ctx.OtherCtx );
			_ = new SkyStretchesToShoreApi( ctx.Other ); // Auto-binds to spirit

			return Task.CompletedTask;
		}

	}

	class SkyStretchesToShoreApi : TargetLandApi {
		public SkyStretchesToShoreApi( Spirit spirit ) {
			this.orig = spirit.TargetLandApi;
			spirit.TargetLandApi = this;
		}

		public override IEnumerable<Space> GetTargetOptions( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum, PowerType powerType ) {
			var normal = orig.GetTargetOptions( self, gameState, sourceEnum, sourceTerrain, range, filterEnum, powerType );
			var shore = orig.GetTargetOptions( self, gameState, sourceEnum, sourceTerrain, range+3, filterEnum, powerType ).Where(x => x.IsCoastal);
			return normal.Union(shore);
		}

		readonly TargetLandApi orig;
	}

}
