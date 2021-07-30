using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class WashAway {

		public const string Name = "Wash Away";

		[SpiritCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
		[FromPresence(1,Filter.TownOrExplorer)]
		static public async Task ActionAsync(ActionEngine engine,Space target){
			var (_,gameState) = engine;
			
			var group = gameState.InvadersOn(target);
			int numToPush = 3;
			Invader[] CalcInvaders() => group.FilterBy(Invader.Town,Invader.Explorer);
			var availableInvaders = CalcInvaders();
			while(numToPush > 0 && availableInvaders.Length>0){
				var invader = await engine.SelectInvader("Select invader to push."
					,availableInvaders
					,true
				);
				if(invader == null) break;
				await engine.PushInvader(group.Space,invader);

				--numToPush;
				group[invader]--;
				availableInvaders = CalcInvaders();
			}
		}

	}

}
