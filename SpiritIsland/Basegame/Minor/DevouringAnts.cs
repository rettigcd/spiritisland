using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class DevouringAnts {

		[MinorCard("Devouring Ants",1,Speed.Slow,Element.Sun,Element.Earth,Element.Animal)]
		[FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			ctx.AddFear(1);
			if(ctx.DahanCount>0)
				await ctx.DestroyDahan(1,DahanDestructionSource.PowerCard);
			int bonusDamage = ctx.IsOneOf(Terrain.Sand,Terrain.Jungle) ? 1 : 0;
			await ctx.DamageInvaders( 1+bonusDamage );
		}

	}

}
