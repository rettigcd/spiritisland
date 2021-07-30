﻿using System;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class HarbingersOfTheLightning {
		public const string Name = "Harbingers of the Lightning";

		[SpiritCard(HarbingersOfTheLightning.Name,0,Speed.Slow,Element.Fire,Element.Air)]
		[FromPresence(1,Filter.Dahan)]
		static public async Task ActionAsync(ActionEngine engine,Space target){

			// Push up to 2 dahan.
			var destinationSpaces = await engine.PushUpToNDahan(target,2);

			// if pushed dahan into town or city
			bool pushedToBuildingSpace = destinationSpaces
				.Where(neighbor => {
					var grp = engine.GameState.InvadersOn(neighbor);
					return grp.HasTown || grp.HasCity;
				})
				.Any();
			if(pushedToBuildingSpace)
				engine.GameState.AddFear(1);
		}



	}
}
