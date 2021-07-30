using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	class DevouringAnts {

		[MinorCard("Devouring Ants",1,Speed.Slow,Element.Sun,Element.Earth,Element.Animal)]
		[FromSacredSite(1)]
		static public void ActAsync(ActionEngine engine,Space target){
			var (_,gs) = engine;
			gs.AddFear(1);
			if(gs.GetDahanOnSpace(target)>0)
				gs.AddDahan(target,-1);
			int bonusDamage = target.Terrain.IsIn(Terrain.Sand,Terrain.Jungle) ? 1 : 0;
			gs.DamageInvaders(target, 1+bonusDamage);
		}

	}

}
