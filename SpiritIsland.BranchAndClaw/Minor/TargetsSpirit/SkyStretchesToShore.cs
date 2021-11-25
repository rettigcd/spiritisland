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
			ctx.GameState.TimePasses_ThisRound.Push( new PowerApiRestorer( ctx.Other ).Restore );
			_ = new SkyStretchesToShoreApi( ctx.Other ); // Auto-binds to spirit

			return Task.CompletedTask;
		}

	}

	class SkyStretchesToShoreApi : DefaultCalcRange {
		public SkyStretchesToShoreApi( Spirit spirit ) {
			this.orig = spirit.RangeCalc;
			spirit.RangeCalc = this;
		}

		public override IEnumerable<Space> GetTargetOptionsFromKnownSource( Spirit self, GameState gameState, int range, string filterEnum, TargettingFrom powerType, IEnumerable<Space> source ) {
			var normal = orig.GetTargetOptionsFromKnownSource( self, gameState, range, filterEnum, powerType, source );
			var shore = orig.GetTargetOptionsFromKnownSource( self, gameState, range+3, filterEnum, powerType, source ).Where(x => x.IsCoastal);
			return normal.Union(shore);
		}

		readonly ICalcRange orig;
	}

}
