﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class SteamVents {


		[MinorCard("Steam Vents", 1, Speed.Fast, "fire,air,water,earth")]
		[FromPresence(0)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			var grp = ctx.Invaders;

			// if your have 3 earth, 
			if( ctx.Self.Elements.Contains("3 earth") && grp.Counts.Has(Invader.Town) )
				// instead destroy 1 town
				await grp.Destroy(1, Invader.Town);
			else
				// destroy 1 explorer
				await grp.Destroy(1, Invader.Explorer);
		}

	}
}
