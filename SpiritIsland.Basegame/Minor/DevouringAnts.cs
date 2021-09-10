using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class DevouringAnts {

		[MinorCard("Devouring Ants",1,Speed.Slow,Element.Sun,Element.Earth,Element.Animal)]
		[FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx){

			// 1 fear
			ctx.AddFear(1);

			//  1 damage
			int damage = 1;

			// destroy 1 dahan
			if(ctx.DahanCount>0)
				await ctx.DestroyDahan(1);

			// if target is J / S, +1 damage
			if( ctx.IsOneOf( Terrain.Jungle, Terrain.Sand ) )
				++damage;

			await ctx.DamageInvaders( damage );
		}

	}

}
