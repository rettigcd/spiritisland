using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {


	public class PillarOfLivingFlame {

		[MajorCard("Pillar of Living Flame",5,Speed.Slow,Element.Fire)]
		static public async Task ActionAsync(ActionEngine engine, Spirit self,GameState gameState){

			var targetLand = await engine.SelectSpace("Select target",
				self.SacredSites.Range(2).Where(s=>s.IsLand)
			);

			// 3 fear, 5 damage
			// if you have 4 fire, +2 fear, +5 damage
			bool hasBonus = self.Elements(Element.Fire)>=4;
			gameState.AddFear( 3 + (hasBonus ? 2 : 0) );
			gameState.DamageInvaders(targetLand, 5 + (hasBonus ? 5 : 0));

			// if target is Jungle / Wetland, add 1 blight
			if(targetLand.Terrain.IsIn(Terrain.Jungle,Terrain.Wetland))
				gameState.AddBlight(targetLand);

		}

	}
}
