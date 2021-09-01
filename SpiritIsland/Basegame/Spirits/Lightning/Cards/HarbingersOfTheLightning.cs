﻿using System;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class HarbingersOfTheLightning {
		public const string Name = "Harbingers of the Lightning";

		[SpiritCard(HarbingersOfTheLightning.Name,0,Speed.Slow,Element.Fire,Element.Air)]
		[FromPresence(1,Target.Dahan)]
		static public async Task ActionAsync(TargetSpaceCtx ctx){

			// Push up to 2 dahan.
			var destinationSpaces = await ctx.PowerPushUpToNDahan(2);

			// if pushed dahan into town or city
			bool pushedToBuildingSpace = destinationSpaces
				.Where(neighbor => {
					var grp = ctx.GameState.Invaders.Counts[neighbor];
					return grp.Has(Invader.Town) || grp.Has(Invader.City);
				})
				.Any();
			if(pushedToBuildingSpace)
				ctx.AddFear(1);
		}



	}
}
