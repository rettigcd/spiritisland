﻿using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class MesmerizedTranquility{ 

		[MinorCard("Mesmerized Tranquility",0,Element.Water,Element.Earth,Element.Animal),Fast,FromPresence(0)]
		static public Task ActAsync( TargetSpaceCtx ctx ){
			// Isolate target land.
			ctx.Isolate();

			// Each Invader does -1 Damage.
			ctx.ModifyRavage( cfg => cfg.DamageFromInvader = (invader) => invader.FullHealth-1 );

			return Task.CompletedTask;
		}
	}



}
