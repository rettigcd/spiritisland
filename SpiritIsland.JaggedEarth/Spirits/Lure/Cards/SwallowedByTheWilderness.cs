﻿using System;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class SwallowedByTheWilderness {
		[SpiritCard("Swallowed by the Wilderness",1,Element.Fire,Element.Air,Element.Plant,Element.Animal),Fast,FromPresence(0,Target.Inland)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			// 2 fear
			ctx.AddFear(2);

			// 1 damage per beast/disease/wilds/badlands.  (Count max. 5 tokens.)
			int damage = Math.Min(5,ctx.Beasts.Count + ctx.Disease.Count + ctx.Wilds.Count + ctx.Badlands.Count);
			return ctx.DamageInvaders(damage);
		}
	}


}
