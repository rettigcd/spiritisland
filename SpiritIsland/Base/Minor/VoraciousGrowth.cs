using System.Threading.Tasks;
using SpiritIsland.Core;


namespace SpiritIsland.Base {
	class VoraciousGrowth {

		[MinorCard("Voracious Growth",1,Speed.Slow,Element.Water,Element.Plant)]
		static public async Task ActAsync(ActionEngine engine){
			var (spirit,gs) = engine;
			var target = await engine.Api.TargetSpace_SacredSite(1,s=>s.Terrain.IsIn(Terrain.Jungle,Terrain.Wetland));

			const string blightKey = "Remove 1 Blight";

			bool removeBlight = gs.HasBlight(target) 
				&& blightKey == await engine.SelectText("Select action","2 Damage",blightKey);

			if(removeBlight)
				gs.AddBlight(target,-1);
			else
				// 2 damage
				gs.DamageInvaders(target,2);
		}

	}

}
