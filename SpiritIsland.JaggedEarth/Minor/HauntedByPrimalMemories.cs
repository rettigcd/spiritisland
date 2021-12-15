﻿using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class HauntedByPrimalMemories{ 
		[MinorCard("Haunted by Primal Memories",1,Element.Moon,Element.Air,Element.Earth),Fast, FromSacredSite(2,Target.Invaders)]
		static public Task ActAsync(TargetSpaceCtx ctx){
			// 1 fear
			ctx.AddFear(1);
			// Defend 3
			ctx.Defend(3);
			// If beast are present, +2 fear.
			if(ctx.Beasts.Any)
				ctx.AddFear(2);

			return Task.CompletedTask;
		}
	}



}
