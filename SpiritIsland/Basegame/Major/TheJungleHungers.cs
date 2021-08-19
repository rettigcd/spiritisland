﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class TheJungleHungers {

		[MajorCard("The Jungle Hungers",3,Speed.Slow,Element.Moon,Element.Plant)]
		[FromPresenceIn(1,Terrain.Jungle)]
		static public async Task Act(TargetSpaceCtx ctx){
			// range 1 from presence in jungle
			var target = await ctx.TargetSpace( From.Presence, null, 1, Target.Jungle );
			var grp = ctx.InvadersOn(target);

			// destroys all explorers and towns
			await grp.Destroy(Invader.Explorer, int.MaxValue);
			await grp.Destroy(Invader.Town, int.MaxValue );

			// Destroy all dahan
			bool destroyAllDahan = true;

			// if you have 2 moon, 3 plant
			if(ctx.Self.Elements.Contains( "2 moon,3 plant" )) {
				// Destroy 1 city
				await grp.Destroy(Invader.City, 1 );
				// do not destroy dahan
				destroyAllDahan = false;
			}

			if(destroyAllDahan)
				await ctx.DestroyDahan( ctx.DahanCount, DahanDestructionSource.PowerCard);

		}

	}
}
