using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class WashAway {

		public const string Name = "Wash Away";

		[SpiritCard(WashAway.Name, 1, Speed.Slow, Element.Water, Element.Earth)]
		static public async Task ActionAsync(ActionEngine engine){
			var (self,gameState) = engine;
			bool HasTownOrExplorer(Space space){
				var sum = gameState.InvadersOn(space);
				return sum.HasExplorer || sum.HasTown;
			}

			var target = await engine.Api.TargetSpace_Presence(1,HasTownOrExplorer);

			var group = gameState.InvadersOn(target);
			int numToPush = 3;
			var availableInvaders = group.Filter("T@2","T@1","E@1");
			while(numToPush > 0 && availableInvaders.Length>0){
				var invader = await engine.SelectInvader("Select invader to push."
					,availableInvaders
					,true
				);
				if(invader == null) break;
				await engine.PushInvader(group.Space,invader);

				--numToPush;
				group[invader]--;
				availableInvaders = group.Filter("T@2","T@1","E@1");
			}
		}

	}

}
