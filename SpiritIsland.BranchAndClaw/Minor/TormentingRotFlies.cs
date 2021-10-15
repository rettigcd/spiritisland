﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class TormentingRotFlies {

		[MinorCard( "Tormenting Rotflies", 1, Element.Air, Element.Plant, Element.Animal )]
		[Slow]
		[FromPresence( 2, Target.SandOrWetland )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( "Add 1 disease", () => ctx.Disease.Count++ ),
				new ActionOption( "2 fear, +1(if disease) +1(if blight)", ()=>AddFear(ctx), ctx.Tokens.HasInvaders() )
			);

		}

		static public void AddFear( TargetSpaceCtx ctx ) {
			int fearCount = 2;
			if( ctx.Disease.Any ) fearCount++;
			if( ctx.Blight>0 ) fearCount++;
			ctx.AddFear( fearCount );
		}
	}
}
