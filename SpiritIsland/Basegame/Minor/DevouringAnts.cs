using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	class DevouringAnts {

		[MinorCard("Devouring Ants",1,Speed.Slow,Element.Sun,Element.Earth,Element.Animal)]
		[FromSacredSite(1)]
		static public Task ActAsync(TargetSpaceCtx ctx){
			var target = ctx.Target;
			var (_,gs) = ctx;
			ctx.AddFear(1);
			if(gs.GetDahanOnSpace(target)>0)
				gs.DestoryDahan(target,1,DahanDestructionSource.PowerCard);
			int bonusDamage = target.Terrain.IsIn(Terrain.Sand,Terrain.Jungle) ? 1 : 0;
			return ctx.DamageInvaders(target, 1+bonusDamage);
		}

	}

}
