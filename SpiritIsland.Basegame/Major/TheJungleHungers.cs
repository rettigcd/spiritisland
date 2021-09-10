using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TheJungleHungers {

		[MajorCard("The Jungle Hungers",3,Speed.Slow,Element.Moon,Element.Plant)]
		[FromPresenceIn(1,Terrain.Jungle)]
		static public async Task ActAsync(TargetSpaceCtx ctx){

			// destroys all explorers and towns
			await ctx.Invaders.Destroy( int.MaxValue, Invader.Explorer);
			await ctx.Invaders.Destroy( int.MaxValue, Invader.Town );

			// Destroy all dahan
			bool destroyAllDahan = true;

			// if you have 2 moon, 3 plant
			if(ctx.YouHave( "2 moon,3 plant" )) {
				// Destroy 1 city
				await ctx.Invaders.Destroy( 1, Invader.City );
				// do not destroy dahan
				destroyAllDahan = false;
			}

			if(destroyAllDahan)
				await ctx.DestroyDahan( ctx.DahanCount );

		}

	}
}
