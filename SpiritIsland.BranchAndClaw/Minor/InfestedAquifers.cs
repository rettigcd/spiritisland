﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InfestedAquifers {

		[MinorCard( "Infested Aquifers", 0, Element.Moon, Element.Water, Element.Earth, Element.Animal )]
		[Slow]
		[FromPresence( 0 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( "1 damage to each invader"
					, () => ctx.DamageEachInvader( 1 )
					, ctx.Disease.Any
				),
				new ActionOption( "1 fear and 1 disease"
					, () => { ctx.AddFear(1); ctx.Disease.Add(1); return Task.CompletedTask; }
					, ctx.IsOneOf(Terrain.Mountain,Terrain.Wetland)
				)
			);
		}

	}

}
