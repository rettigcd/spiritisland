using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class CallToBloodshed {
		[MinorCard("Call to Bloodshed",1,Speed.Slow,Element.Sun,Element.Fire,Element.Animal)]
		static public async Task Act(ActionEngine engine){
			var (self,gameState)=engine;
			// range 2, dahan
			var target = await engine.SelectSpace("Select target"
				,self.Presence.Range(2).Where(gameState.HasDahan)
			);

			const string key1 = "1 damage per dahan";
			const string key2 = "gather up to 3 dahan";
			string opt = await engine.SelectText("Select option",key1,key2);

			if(opt==key1){
				// opt 1 - 1 damage per dahan
				gameState.DamageInvaders(target,gameState.GetDahanOnSpace(target));
				return;
			}

			// opt 2 - gather up to 3 dahan
			int gatherMax = 3, gathered = 0;
			var neighborsWithDahan = target.Neighbors.Where(gameState.HasDahan).ToArray();
			while(gathered<gatherMax && neighborsWithDahan.Length>0){
				var source = await engine.SelectSpace( $"Gather ({gathered+1} of {gatherMax}) dahan from:", neighborsWithDahan, true);
				if(source == null) break;

				gameState.AddDahan(source,-1);
				gameState.AddDahan(target,1);

				++gathered;
				neighborsWithDahan = target.Neighbors.Where(gameState.HasDahan).ToArray();
			}
			
		}
	}
}
